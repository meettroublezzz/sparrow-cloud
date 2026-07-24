using SparrowCloud.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Storage
{
    internal class StorageTool
    {
        internal static dynamic GetFileThumbnail(StorageFileThumbnail thumbnail)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 裁剪描述信息
        /// </summary>
        /// <param name="remark"></param>
        /// <returns></returns>
        internal static string TailorRemark(string remark)
        {
            string[] strings = remark.Split('\n');

            return strings[0].Trim();
        }
    }
}
