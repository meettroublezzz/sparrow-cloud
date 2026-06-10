using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models
{
    /// <summary>
    /// 所有实体基类
    /// </summary>
    public abstract class EntityBase<TKeyType>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public virtual required TKeyType Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateAt { get; set; } = EntityBase.NowTicks;

        /// <summary>
        /// 最后修改日期
        /// </summary>
        public long? UpdateAt { get; set; }
    }

    /// <summary>
    /// 实体基类工具
    /// </summary>
    public static class EntityBase
    {
        /// <summary>
        /// 新生成GUID值（36 个字符）
        /// </summary>
        /// <returns></returns>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 获取当前时间戳（当前时区，非UTC）
        /// </summary>
        public static long NowTicks { get => DateTime.Now.Ticks; }

        /// <summary>
        /// 获取当前时间戳（当前时区，非UTC）
        /// </summary>
        public static long UtcNowTicks { get => DateTime.UtcNow.Ticks; }
    }

    /// <summary>
    /// 所有实体基类（自增主键版）
    /// </summary>
    public abstract class EntityIncr<TKeyType> : EntityBase<TKeyType> where TKeyType : struct, IComparable, IConvertible, IFormattable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override required TKeyType Id { get; set; }
    }

    /// <summary>
    /// 所有实体基类（自增 int 版）
    /// </summary>
    public abstract class EntityIncr : EntityIncr<int>
    {
        
    }
}
