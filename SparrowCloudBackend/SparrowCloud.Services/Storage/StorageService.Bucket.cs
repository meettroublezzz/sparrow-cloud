using Microsoft.EntityFrameworkCore;
using SparrowCloud.Models;
using SparrowCloud.Models.Storage;
using SparrowCloud.Utils;
using SparrowCloud.Utils.CustomExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SparrowCloud.Services.Storage
{
    /*
     * 桶文件相关
     */
    public partial class StorageService
    {
        // 最大长度（约8MB）
        private const long BucketMaxLength = 8388608;

        /// <summary>
        /// 上传桶文件
        /// </summary>
        /// <param name="length"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<string> UploadBucketFileAsync(string bucketName, long length, Stream stream, string fileName)
        {
            /*
             * 长度小于阈值的文件，直接压缩后存数据库；
             * 大文件则存到文件系统里。
             */

            string extension = Path.GetExtension(fileName);

            var bucket = new StorageBucket()
            {
                Id = default,

                BucketName = bucketName,
                ObjectName = $"{EntityBase.GenerateGuid()}{extension}",
                Extension = extension,
            };

            string filePath = Path.Combine(_bucketWorkPath, bucket.BucketName, bucket.ObjectName[..2], $"{bucket.ObjectName}");

            // 确保文件夹存在，后续要用
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            if (length <= BucketMaxLength)
            {
                byte[] data = await stream.ToByteArrayAsync();

                if (data.Length != length)
                    throw new ServiceException("桶文件上传长度不相等！");

                bucket.BinaryData = CompressionHelper.CompressBytes(data);
            }
            else
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);

                    if (fileStream.Length != length)
                        throw new ServiceException("桶文件上传长度不相等！");
                }
            }

            using (var db = GetStorageContext())
            {
                db.StorageBuckets.Add(bucket);

                await db.SaveChangesAsync();
            }

            return $"/buckets/{_metadata.StorageGuid}/{bucket.BucketName}/{bucket.ObjectName}";
        }

        /// <summary>
        /// 下载桶文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public async Task<(Stream stream, string contentType)> DownBucketFileAsync(string bucketName, string objectName)
        {
            using var db = GetStorageContext();

            var bucket = await db.StorageBuckets
                .Where(e => e.BucketName == bucketName && e.ObjectName == objectName)
                .SingleOrDefaultAsync();

            if (bucket == null)
                throw new ServiceException("桶文件不存在", 404);

            var contentType = HttpHelper.GetContentTypeByExtension(bucket.Extension);

            Stream stream;

            if (bucket.BinaryData != null)
            {
                stream = new MemoryStream(CompressionHelper.DecompressBytes(bucket.BinaryData));
            }
            else
            {
                string filePath = Path.Combine(_bucketWorkPath, bucket.BucketName, bucket.ObjectName[..2], $"{bucket.ObjectName}");

                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            return (stream, contentType);
        }

        /// <summary>
        /// 删除桶文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public async Task DelBucketFileAsync(string bucketName, string objectName)
        {
            using var db = GetStorageContext();

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var rows = await db.StorageBuckets
                    .Where(e => e.BucketName == bucketName && e.ObjectName == objectName)
                    .ExecuteDeleteAsync();

                if (rows == 0)
                    throw new ServiceException("桶文件不存在", 404);

                string filePath = Path.Combine(_bucketWorkPath, bucketName, objectName[..2], $"{objectName}");

                File.Delete(filePath);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                
                throw;
            }
        }
    }
}
