using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.StorageModels
{
    public class BaseInfoModel
    {
        /// <summary>
        /// 封面
        /// </summary>
        public required string CoverUrl { get; set; }

        /// <summary>
        /// 名称（最优先展示的）
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 星级（满十分）半星=1分
        /// </summary>
        public byte StarLevel { get; set; }

        /// <summary>
        /// 收藏（喜爱）
        /// null 表达未收藏
        /// </summary>
        public DateTime? FavoritedTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime? LastAccessAt { get; set; }

        /// <summary>
        /// 标签展示
        /// </summary>
        public required IEnumerable<BaseInfoTag> Tags { get; set; }
    }

    public class BaseInfoTag
    {
        /// <summary>
        /// 标签ID
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 标签颜色（十六进制，如 #ff0000）前端展示用
        /// </summary>
        public string? Color { get; set; }
    }
}
