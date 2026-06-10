using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SparrowCloud.Models.CustomExtensions
{
    /// <summary>
    /// EF Core SQLite 高性能批量插入扩展（泛型版本）
    /// 核心技术：预处理语句 + 参数复用 + 显示事务 + 表达式树编译委托 + 纯同步执行
    /// 目标性能：200万条/秒以上
    /// </summary>
    public static class EFCoreSqliteBulkInsertExtension
    {
        /// <summary>
        /// SQLite 高性能批量插入
        /// 推荐批次大小：1万条/批
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="context">数据库上下文</param>
        /// <param name="entities">要插入的实体集合</param>
        /// <param name="excludeProperties">需要排除的属性名称</param>
        /// <returns>插入的记录数</returns>
        public static int SqliteBulkInsert<TEntity>(this DbContext context, IEnumerable<TEntity> entities, params string[] excludeProperties) where TEntity : class
        {
            // 空值校验
            if (entities == null)
            {
                return 0;
            }

            // 获取第一个元素，用于提取属性信息
            var firstEntity = entities.FirstOrDefault();
            if (firstEntity == null)
            {
                return 0;
            }

            // 获取需要排除的字段名称
            var excludeNames = new HashSet<string>(excludeProperties ?? Array.Empty<string>());

            // 自动排除 [NotMapped] 和 virtual 标记的属性
            var entityType = firstEntity.GetType();
            foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // 排除 [NotMapped] 标记的属性
                if (Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute)))
                {
                    excludeNames.Add(prop.Name);
                }
                // 排除 virtual 标记的导航属性
                else if (prop.GetMethod?.IsVirtual == true)
                {
                    excludeNames.Add(prop.Name);
                }
            }

            // 从第一个实体获取属性信息（属性名 + 编译委托）
            var propertyGetters = CompileProperties(entityType, excludeNames, out string[] columnNames);

            // 获取表名（通过 EF Core 元数据获取真实表名）
            var tableName = context.Model.FindEntityType(typeof(TEntity))?.GetTableName() ?? typeof(TEntity).Name;

            // 获取数据库连接
            var connection = (SqliteConnection)context.Database.GetDbConnection();

            // 执行批量插入
            var insertCount = ExecuteBulkInsert(connection, tableName, columnNames, propertyGetters, entities);

            return insertCount;
        }

        /// <summary>
        /// 为实体的所有属性（排除指定字段）编译快速读取委托
        /// </summary>
        private static Func<object, object>[] CompileProperties(Type entityType, HashSet<string> excludeNames, out string[] columnNames)
        {
            // 获取所有公共实例属性（排除需要排除的字段）
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !excludeNames.Contains(p.Name))
                .ToArray();

            // 保存列名数组（使用属性名作为列名）
            columnNames = properties.Select(p => p.Name).ToArray();

            // 为每个属性编译快速读取委托
            var getters = new Func<object, object>[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                getters[i] = CompilePropertyGetter(properties[i]);
            }

            return getters;
        }

        /// <summary>
        /// 为单个属性编译表达式树委托
        /// 生成的委托等价于：(entity) => (object)entity.PropertyName
        /// </summary>
        private static Func<object, object> CompilePropertyGetter(PropertyInfo property)
        {
            // 创建参数表达式：entity
            var paramExpr = Expression.Parameter(typeof(object), "entity");

            // 类型转换表达式：(EntityType)entity
            var castExpr = Expression.Convert(paramExpr, property.DeclaringType!);

            // 属性访问表达式：entity.Property
            var propertyAccessExpr = Expression.Property(castExpr, property);

            // 装箱表达式（值类型转 object）：(object)entity.Property
            var boxExpr = Expression.Convert(propertyAccessExpr, typeof(object));

            // 构建 Lambda 表达式：entity => (object)((EntityType)entity).Property
            var lambdaExpr = Expression.Lambda<Func<object, object>>(boxExpr, paramExpr);

            // 编译成可执行委托
            var getter = lambdaExpr.Compile();

            return getter;
        }

        /// <summary>
        /// 执行批量插入的核心方法（纯同步）
        /// 使用预处理语句 + 参数复用实现极致性能
        /// </summary>
        private static int ExecuteBulkInsert<TEntity>(
            SqliteConnection connection,
            string tableName,
            string[] columnNames,
            Func<object, object>[] propertyGetters,
            IEnumerable<TEntity> entities)
        {
            int insertCount = 0;

            // 打开数据库连接（如果尚未打开）
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            // 开启显示事务（同步）
            // 事务是批量插入性能的关键：避免每条记录都触发磁盘 IO 和日志刷盘
            using var transaction = connection.BeginTransaction();

            try
            {
                // 构建单条插入 SQL
                var sql = BuildInsertSql(tableName, columnNames);

                // 创建 SQLite 命令对象（只创建一次）
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Transaction = transaction;

                // 预先创建所有参数对象（只创建一次）
                var parameters = CreateParameters(command, columnNames.Length);

                // 预处理 SQL 语句（核心优化：将 SQL 编译成字节码，跳过语法分析）
                command.Prepare();

                // 遍历所有数据，逐条绑定参数值并执行（纯同步）
                foreach (var entity in entities)
                {
                    // 跳过空值
                    if (entity == null)
                    {
                        continue;
                    }

                    // 更新参数值（复用已有参数对象，只更新 Value）
                    UpdateParameterValues(parameters, propertyGetters, entity);

                    // 执行插入语句（预处理后执行极快）
                    command.ExecuteNonQuery();

                    // 计数加一
                    insertCount++;
                }

                // 提交事务
                transaction.Commit();
            }
            catch
            {
                // 发生异常时回滚事务
                transaction.Rollback();
                throw;
            }

            return insertCount;
        }

        /// <summary>
        /// 构建单条 INSERT SQL 语句
        /// 示例：INSERT INTO Table (Col1, Col2) VALUES (@p0, @p1);
        /// </summary>
        private static string BuildInsertSql(string tableName, string[] columnNames)
        {
            // 构建列名部分：(Column1, Column2, Column3)
            var columnNamesStr = string.Join(", ", columnNames);

            // 构建参数部分：(@p0, @p1, @p2)
            var parameterNames = string.Join(", ", Enumerable.Range(0, columnNames.Length).Select(i => $"@p{i}"));

            // 组合成完整 SQL
            var sql = $"INSERT INTO {tableName} ({columnNamesStr}) VALUES ({parameterNames});";

            return sql;
        }

        /// <summary>
        /// 预先创建所有参数对象（只创建一次）
        /// </summary>
        private static List<SqliteParameter> CreateParameters(SqliteCommand command, int columnCount)
        {
            var parameters = new List<SqliteParameter>(columnCount);

            // 为每个列创建一个参数对象
            for (int i = 0; i < columnCount; i++)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@p{i}";
                
                // 初始值设为 DBNull.Value
                parameter.Value = DBNull.Value;

                // 将参数添加到命令中
                command.Parameters.Add(parameter);

                // 保存参数引用，方便后续更新值
                parameters.Add(parameter);
            }

            return parameters;
        }

        /// <summary>
        /// 更新参数值（复用已有参数对象，只更新 Value）
        /// 处理空值情况：null 值转换为 DBNull.Value
        /// </summary>
        private static void UpdateParameterValues(
            List<SqliteParameter> parameters,
            Func<object, object>[] propertyGetters,
            object entity)
        {
            // 遍历所有参数
            for (int i = 0; i < parameters.Count; i++)
            {
                var getter = propertyGetters[i];
                var parameter = parameters[i];

                // 使用编译好的委托获取属性值（无反射开销）
                var value = getter(entity);

                // 空值处理：C# 的 null 必须转换为 DBNull.Value 才能插入数据库
                parameter.Value = value ?? (object)DBNull.Value;
            }
        }
    }
}
