using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SparrowCloud.Contracts.StorageModels;
using SparrowCloud.Models.Union;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Storage
{
    /*
	 * 放置右侧信息处理逻辑
	 */
    public partial class StorageService
    {
        /// <summary>
        /// 获取文件库的 首栏-基本信息
        /// </summary>
        /// <returns></returns>
        public async Task<BaseInfoModel> GetStorageBaseInfoAsync(UnionStorage entity, bool isFull = false)
        {
            using var db = GetStorageContext();

            if (isFull)
            {
                var json = (await GetConfigItemByKey("storage_tags")).ConfigValue!;
                var tagsTemp = JsonConvert.DeserializeObject<string[]>(json)!;
                var tags = tagsTemp
                    .Select(e => new BaseInfoTag()
                    {
                        TagId = -1,
                        Name = e,
                        Color = null,
                    })
                    .AsEnumerable();

                return new()
                {
                    CoverUrl = (await GetConfigItemByKey("storage_cover_url")).ConfigValue!,

                    Name = entity.Name,

                    StarLevel = (await GetConfigItemByKey("storage_info")).StarLevel,
                    FavoritedTime = (await GetConfigItemByKey("storage_info")).FavoritedTime,

                    CreatedAt = _metadata.CreateAt,
                    UpdatedAt = entity.UpdateTime,
                    LastAccessAt = entity.LastAccessAt,

                    Tags = tags,
                };
            }
            else
            {
                return new()
                {
                    CoverUrl = (await GetConfigItemByKey("storage_cover_url")).ConfigValue!,

                    Name = entity.Name,

                    CreatedAt = _metadata.CreateAt,
                    UpdatedAt = entity.UpdateTime,
                    LastAccessAt = entity.LastAccessAt,

                    Tags = [],
                };
            }
        }

        /// <summary>
        /// 获取文件库描述
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetStorageRemarkAsync()
        {
            var remark = (await GetConfigItemByKey("storage_info")).Remark!;

            return remark;
        }
        /// <summary>
        /// 设置文件库描述
        /// </summary>
        /// <param name="remark"></param>
        /// <returns></returns>
        public async Task SetStorageRemarkAsync(string remark)
        {
            using (var db = GetStorageContext())
            {
                await db.StorageConfigs
                    .Where(e => e.ConfigKey == "storage_info")
                    .ExecuteUpdateAsync(e => e
                        .SetProperty(s => s.Remark, remark)
                    );

                // 清缓存
                configsCache = null;
                _ = Task.Delay(1500).ContinueWith(_ =>
                {
                    configsCache = null;
                });
            }
        }

        /// <summary>
        /// 获取文件库大小
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetStorageSizeAsync()
        {
            using (var db = GetStorageContext())
            {
                long size = await db.StorageFiles
                    .Where(e => !e.IsDirectory)
                    .SumAsync(e => e.FileLength);

                return size;
            }
        }
    }
}
