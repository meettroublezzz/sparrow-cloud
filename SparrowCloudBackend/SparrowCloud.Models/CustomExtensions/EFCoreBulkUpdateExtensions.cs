using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.CustomExtensions
{
    /// <summary>
    /// EF Core 原生批量更新扩展（适配匿名类型 + List<object>）
    /// 无强类型、无配置类、无事务、自动分4096条/批
    /// </summary>
    public static class EfCoreBulkUpdateExtensions
    {
        // 固定批次大小：4096
        private const int BatchSize = 4096;

        /// <summary>
        /// 根据多条件值批量更新（无事务）
        /// </summary>
        /// <typeparam name="T">表类型</typeparam>
        /// <param name="context"></param>
        /// <param name="keyColumns">条件列名</param>
        /// <param name="updateColumns">要更新的列名</param>
        /// <param name="entities">批量更新数据项</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task BulkUpdateAsync<T>(this DbContext context, string[] keyColumns, string[] updateColumns, List<object> entities) where T : class
        {
            // 空值校验
            if (keyColumns == null || keyColumns.Length == 0
                || updateColumns == null || updateColumns.Length == 0
                || entities == null || entities.Count == 0)
            {
                return;
            }

            // 强类型自动获取表名
            var tableName = context.Model.FindEntityType(typeof(T))?.GetTableName()
                            ?? throw new InvalidOperationException($"无法获取实体 {typeof(T).Name} 对应的数据表名");

            // 获取数据库连接
            await using var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                // 自动分批次：4096条一组执行
                foreach (var batch in entities.Chunk(BatchSize))
                {
                    await ExecuteBatchAsync(connection, tableName, keyColumns.ToList(), updateColumns.ToList(), batch);
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        #region 核心：执行单批次更新（参数化SQL）
        private static async Task ExecuteBatchAsync(
            DbConnection connection,
            string tableName,
            List<string> keyColumnNames,
            List<string> updateColumnNames,
            object[] batchEntities)
        {
            // 构建 SQLite 参数化 UPDATE 语句
            var setClause = string.Join(", ", updateColumnNames.Select(col => $"{col} = @{col}"));
            var whereClause = string.Join(" AND ", keyColumnNames.Select(col => $"{col} = @{col}"));
            var sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause};";

            // 逐条执行当前批次（参数化，无SQL注入）
            foreach (var entity in batchEntities)
            {
                if (entity == null) continue;

                await using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;

                // 合并条件列 + 更新列，统一赋值参数
                var allColumns = keyColumnNames.Concat(updateColumnNames);
                foreach (var columnName in allColumns)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = $"@{columnName}";

                    // 反射读取匿名对象的属性值
                    var value = entity.GetType().GetProperty(columnName)?.GetValue(entity) ?? DBNull.Value;
                    param.Value = value;

                    cmd.Parameters.Add(param);
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }
        #endregion
    }
}
