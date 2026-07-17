using SparrowCloud.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.MountModels
{
    public class MetadataModel
    {
        /// <summary>
        /// 目录条目
        /// </summary>
        public IEnumerable<DirectoryEntry>? Entry { get; set; }

        /// <summary>
        /// 父目录下的所有子目录
        /// </summary>
        public IEnumerable<DirectoryTreeNode>? Tree { get; set; }
    }
}
