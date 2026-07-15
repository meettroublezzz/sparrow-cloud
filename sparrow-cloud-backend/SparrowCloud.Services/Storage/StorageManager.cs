using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SparrowCloud.Models;
using SparrowCloud.Models.Union;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Storage
{
    public class StorageManager
    {
        #region 相关约定名称
        /// <summary>
        /// 数据仓库名称
        /// </summary>
        public const string StandaloneDirectoryName = @"__SparrowCloud__DWH__Standalone__";

        /// <summary>
        /// 元数据文件名
        /// </summary>
        public const string MetadataFileName = @"metadata.json";

        /// <summary>
        /// 麻雀云盘数据目录（区分小程序）
        /// </summary>
        public const string SparrowCloudName = @"__SparrowCloud__";
        #endregion

        private readonly ILogger<StorageManager> _logger;

        private readonly UnionContext _db;

        private readonly IConfiguration _configuration;

        public StorageManager(ILogger<StorageManager> logger, IConfiguration configuration, UnionContext db)
        {
            _logger = logger;
            _configuration = configuration;

            _db = db;
        }

        /// <summary>
        /// 所有用户的文件库合集：key: $"{uid}+{sid}"， value: 文件库服务对象；
        /// </summary>
        private static readonly ConcurrentDictionary<string, StorageService> _storages = new();

        /// <summary>
        /// 初始化文件库管理器
        /// </summary>
        public static async Task InitAsync(UnionContext db)
        {
            /*
             * 从数据库读取当前所有挂载的文件库，初始化管理器；
             */

            var dataset = db.UnionStorages
                    .Where(e => e.Missing == null && e.Damaged == null)
                    .ToArray();

            foreach (var item in dataset)
            {
                string rootPath = Path.TrimEndingDirectorySeparator(item.RootPath);

                string basePath = Path.Combine(rootPath, StandaloneDirectoryName);

                string metadataPath = Path.Combine(basePath, MetadataFileName);

                #region 检测文件库状态
                // 如果文件库目录丢了
                if (!Directory.Exists(rootPath))
                {
                    // 文件库的目录找不到了，缺失
                    item.Missing = DateTime.Now;

                    continue;
                }

                // 如果数据仓库丢了
                if (!Directory.Exists(basePath))
                {
                    item.Damaged = $"{DateTime.Now}：此文件库损坏`{basePath}`无法找到数据仓库，请检查该文件库！";

                    continue;
                }

                // 如果元数据丢了
                if (!File.Exists(metadataPath))
                {
                    item.Damaged = $"{DateTime.Now}：此文件库损坏`{metadataPath}`内部元数据丢失，无法识别文件库！";

                    continue;
                }
                #endregion

                // 读取元数据
                var metadata = JsonConvert.DeserializeObject<StorageMetadata>(File.ReadAllText(metadataPath))!;

                // 实例化文件库
                StorageService storage = new(rootPath, metadata);

                _storages.TryAdd($"{item.UserId}+{metadata.StorageGuid}", storage);
            }

            db.SaveChanges();
        }

        /// <summary>
        /// 查询文件库信息
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> QueryStorageAsync(string uid)
        {
            string[] storageIdArray = _storages.Keys
                .Where(e => e.StartsWith($"{uid}"))
                .Select(e => e.Split('+')[1])
                .ToArray();

            return await _db.UnionStorages
                .Where(e => e.UserId == uid)
                .OrderByDescending(e => e.Sequence)
                .Select(e => new
                {
                    Id = e.StorageId,
                    e.Name,

                    ready = storageIdArray.Contains(e.StorageId),

                    e.RootPath,
                    e.CreateAt,
                    e.Missing,
                    e.Damaged,
                    e.Sequence,
                    e.LastScan,
                })
                .ToArrayAsync();
        }

        /// <summary>
        /// 获取某个用户的文件库最近的次序
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<double> GetLastSequenceAsync(string uid)
        {
            return await _db.UnionStorages
                .Where(x => x.UserId == uid)
                .Select(x => (double?)x.Sequence)
                .MaxAsync() ?? 0;
        }

        /// <summary>
        /// 创建或附加一个文件库
        /// </summary>
        /// <returns></returns>
        public async Task<string> CreateOrAttachAsync(string uid, string creator, string path)
        {
            string rootPath = Path.TrimEndingDirectorySeparator(path);

            if (!Directory.Exists(rootPath))
                throw new ServiceException("此文件库路径实际不存在！", 404);

            string name = Path.GetFileName(rootPath);

            // 此文件库是否已在数据库内
            bool any = await _db.UnionStorages.AnyAsync(e => e.UserId == uid && e.RootPath == rootPath);

            if (any)
            {
                throw new ServiceException("此文件库已经加载过了(PATH)");
            }

            // 文件库/数据仓库/
            string basePath = Path.Combine(rootPath, StandaloneDirectoryName);

            // 元数据
            StorageMetadata metadata;

            // 这个文件库里是否存在数据仓库
            if (Directory.Exists(basePath))
            {
                metadata = await AttachStorageAsync(basePath, uid);
            }
            else
            {
                metadata = await CreateStorageAsync(basePath, uid, creator);
            }

            double seq = await GetLastSequenceAsync(uid);

            UnionStorage model = new()
            {
                Id = default,
                Name = name,

                UserId = uid,
                StorageId = metadata.StorageGuid,

                RootPath = rootPath,

                Sequence = seq + EntityBase.SequenceStepSize,
            };

            // 添加入库
            _db.UnionStorages.Add(model);
            _db.SaveChanges();

            // 实例化文件库
            StorageService storage = new(rootPath, metadata);

            _storages.TryAdd($"{uid}+{metadata.StorageGuid}", storage);

            return model.StorageId;
        }

        /// <summary>
        /// 创建文件库
        /// </summary>
        /// <param name="basePath">文件库/数据仓库/</param>
        /// <returns></returns>
        private async Task<StorageMetadata> CreateStorageAsync(string basePath, string creatorGuid, string creatorName)
        {
            // 幂等的自动创建所有不存在的文件夹
            Directory.CreateDirectory(basePath);

            StorageMetadata metadata = new()
            {
                Version = Version.Parse(_configuration["SparrowCloud:SystemVersion"]!),

                StorageGuid = EntityBase.GenerateGuid(),

                CreateAt = DateTime.Now,

                CreatorGuid = creatorGuid,
                CreatorName = creatorName,
            };

            string metadataPath = Path.Combine(basePath, MetadataFileName);

            // 保存元数据
            await File.WriteAllTextAsync(metadataPath, JsonConvert.SerializeObject(metadata, Formatting.Indented));

            return metadata;
        }

        /// <summary>
        /// 附加文件库
        /// </summary>
        /// <param name="basePath">文件库/数据仓库/</param>
        /// <returns></returns>
        private async Task<StorageMetadata> AttachStorageAsync(string basePath, string uid)
        {
            // 检测数据仓库的状态
            string metadataPath = Path.Combine(basePath, MetadataFileName);

            if (!File.Exists(metadataPath))
            {
                throw new ServiceException("无法附加：此文件库损坏，内部元数据丢失，无法识别文件库！");
            }

            // 读取元数据
            var metadata = JsonConvert.DeserializeObject<StorageMetadata>(File.ReadAllText(metadataPath))!;

            // 此文件库是否已在数据库内
            bool any = await _db.UnionStorages.AnyAsync(e => e.UserId == uid && e.StorageId == metadata.StorageGuid);

            if (any)
            {
                throw new ServiceException("此文件库已经加载过了(GUID)");
            }

            return metadata;
        }
    
        /// <summary>
        /// 移除某个文件库（并不会删除实际目录或文件）
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RemoveStorageAsync(string uid, string storageId)
        {
            UnionStorage record = await _db.UnionStorages.SingleAsync(e => e.UserId == uid && e.StorageId == storageId);

            _db.UnionStorages.Remove(record);
            
            await _db.SaveChangesAsync();

            _storages.Remove($"{uid}+{record.StorageId}", out _);
        }
        
        /// <summary>
        /// 获取文件库对象
        /// </summary>
        /// <param name="storageId"></param>
        /// <returns></returns>
        public static StorageService GetStorageService(string uid, string storageId)
        {
            var ok = _storages.TryGetValue($"{uid}+{storageId}", out var storage);

            if (!ok)
            {
                throw new ServiceException("此文件库ID不存在");
            }

            return storage!;
        }

        /// <summary>
        /// 扫描遍历文件库（幂等）
        /// </summary>
        /// <returns></returns>
        public async Task ScanFilesAsync(string uid, string storageId)
        {
            var storage = GetStorageService(uid, storageId);

            await storage.ScanFilesAsync();

            // 记录扫描时间
            await _db.UnionStorages
                        .Where(e => e.UserId == uid && e.StorageId == storageId)
                        .ExecuteUpdateAsync(e => e
                            .SetProperty(s => s.LastScan, DateTime.Now)
                        );
        }
    }
}
