using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.MountModels
{
    public class MountAddReq
    {
        /// <summary>
        /// 挂载点名称
        /// </summary>
        [StringLength(127)]
        public required string Name { get; set; }

        /// <summary>
        /// 挂载点路径
        /// </summary>
        [StringLength(4095)]
        public required string Path { get; set; }

        /// <summary>
        /// 挂载点描述
        /// </summary>
        [StringLength(4095)]
        public required string Describe { get; set; }

        /// <summary>
        /// 将Path路径标准化处理
        /// </summary>
        public void NormalizePath()
        {
            if (string.IsNullOrWhiteSpace(Path))
                throw new ArgumentNullException(nameof(Path));

            Path = System.IO.Path.GetFullPath(Path);
        }
    }
}
