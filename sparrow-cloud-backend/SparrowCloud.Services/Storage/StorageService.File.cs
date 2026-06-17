using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Storage
{
    /*
     * 文件&目录 操作
     */
    public partial class StorageService
    {
        /// <summary>
        /// 完全删掉某个文件（不是暂存回收站）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteFileByIdAsync(long id)
        {
            using var db = GetStorageContext();

            var file = await db.StorageFiles.SingleAsync(e => e.Id == id);

            string filePath = Path.Combine(_rootPath, file.RelativeFullPath);
            Console.WriteLine($"del file -> {filePath}");
            //File.Delete(filePath);

            db.StorageFiles.Remove(file);

            await db.SaveChangesAsync();
        }
    }
}
