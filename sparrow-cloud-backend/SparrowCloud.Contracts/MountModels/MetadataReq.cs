using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.MountModels
{
    public class MetadataReq
    {
        public enum MetadataType
        {
            /// <summary>
            /// 常见入口路径
            /// </summary>
            Entry = 1,

            /// <summary>
            /// 具体路径下的目录树
            /// </summary>
            Tree = 2,
        }
    }
}
