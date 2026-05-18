using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SparrowCloud.Models;
using SparrowCloud.Models.Intermediate;
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



        private readonly ILogger<StorageManager> _logger;

        private readonly IntermediateContext _db;

        private readonly IConfiguration _configuration;

        public StorageManager(ILogger<StorageManager> logger, IConfiguration configuration, IntermediateContext db)
        {
            _logger = logger;
            _configuration = configuration;

            _db = db;

            if (_storages == null)
            {
                Init(db);
            }
        }

        /// <summary>
        /// 文件库合集 key:文件库唯一标识; val:文件库服务;（所有用户的）
        /// </summary>
        private static ConcurrentDictionary<string, StorageService>? _storages = null;
        /// <summary>
        /// 读写锁（仅初始化时使用）
        /// </summary>
        private static readonly ReaderWriterLockSlim _lock = new();

        /// <summary>
        /// 初始化文件库管理器
        /// </summary>
        private static void Init(IntermediateContext db)
        {
            /*
             * 从数据库读取当前所有挂载的文件库，初始化管理器；
             */

            // 获取写锁
            _lock.EnterWriteLock();
            try
            {
                // 再次判断是否要初始化
                if (_storages != null)
                    // 已经被初始化过了
                    return;

                // 实例化
                _storages = new();

                var dataset = db.IntermStorages
                    .Where(e => e.Missing == null && e.Damaged == null)
                    .ToArray();

                foreach ( var item in dataset )
                {
                    string rootPath = Path.TrimEndingDirectorySeparator(item.RootPath);

                    string basePath = Path.Combine(rootPath, StandaloneDirectoryName);

                    string metadataPath = Path.Combine(basePath, MetadataFileName);

                    #region 检测文件库状态
                    Console.WriteLine(rootPath);
                    Console.WriteLine(basePath);
                    Console.WriteLine(metadataPath);

                    if (!Directory.Exists(rootPath))
                    {
                        // 文件库的目录找不到了，缺失
                        item.Missing = DateTime.Now;

                        continue;
                    }

                    if (!Directory.Exists(basePath))
                    {
                        item.Damaged = $"{DateTime.Now}：此文件库损坏`{basePath}`无法找到数据仓库，请检查该文件库！";

                        continue;
                    }

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

                    _storages.TryAdd(metadata.StorageGuid, storage);
                }

                db.SaveChanges();
            }
            finally
            {
                // 释放写锁
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 查询文件库信息
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> QueryStorageAsync(string uid)
        {
            string[] guids = _storages!.Keys.ToArray();

            return await _db.IntermStorages
                .Where(e => e.UserId == uid)
                .OrderBy(e => e.Sequence)
                .Select(e => new
                {
                    e.Id,
                    e.Name,

                    ready = guids.Contains(e.StorageId),

                    e.StorageId,
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
            return await _db.IntermStorages
                .Where(x => x.UserId == uid)
                .OrderBy(x => x.Sequence) // 按拖拽顺序排序
                .Select(x => x.Sequence)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// 创建或附加一个文件库
        /// </summary>
        /// <returns></returns>
        public async Task<string> CreateOrAttachAsync(string uid, string creator, string path)
        {
            string rootPath = Path.TrimEndingDirectorySeparator(path);

            string name = Path.GetFileName(rootPath);

            // 此文件库是否已在数据库内
            bool any = await _db.IntermStorages.AnyAsync(e => e.UserId == uid && e.RootPath == rootPath);

            if (any)
            {
                throw new ServiceException("此文件库已经加载过了");
            }

            // 文件库/数据仓库/
            string basePath = Path.Combine(rootPath, StandaloneDirectoryName);

            // 元数据
            StorageMetadata metadata;

            // 这个文件库里是否存在数据仓库
            if (Directory.Exists(basePath))
            {
                metadata = await AttachStorageAsync(basePath);

                // 此文件库是否已在数据库内
                any = await _db.IntermStorages.AnyAsync(e => e.UserId == uid && e.StorageId == metadata.StorageGuid);

                if (any)
                {
                    throw new ServiceException("此文件库已经加载过了");
                }
            }
            else
            {
                metadata = await CreateStorageAsync(basePath, uid, creator);
            }

            double seq = await GetLastSequenceAsync(uid);

            IntermStorage model = new()
            {
                Id = EntityBase.GenerateGuid(),
                Name = name,

                UserId = uid,
                StorageId = metadata.StorageGuid,

                RootPath = rootPath,

                Sequence = seq + IntermStorage.SequenceStepSize,
            };

            // 添加入库
            _db.IntermStorages.Add(model);
            _db.SaveChanges();

            // 实例化文件库
            StorageService storage = new(rootPath, metadata);

            _storages!.TryAdd(metadata.StorageGuid, storage);

            return model.Id;
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
                Version = Version.Parse(_configuration["SparrowVersion"]!),

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
        private async Task<StorageMetadata> AttachStorageAsync(string basePath)
        {
            // 检测数据仓库的状态
            string metadataPath = Path.Combine(basePath, MetadataFileName);

            if (!File.Exists(metadataPath))
            {
                throw new ServiceException("无法附加：此文件库损坏，内部元数据丢失，无法识别文件库！");
            }

            // 读取元数据
            var metadata = JsonConvert.DeserializeObject<StorageMetadata>(File.ReadAllText(metadataPath))!;

            return metadata;
        }
    
        /// <summary>
        /// 移除某个文件库（并不会删除实际目录或文件）
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RemoveStorageAsync(string uid, string id)
        {
            IntermStorage record = await _db.IntermStorages.SingleAsync(e => e.UserId == uid && e.Id == id);

            _db.IntermStorages.Remove(record);
            
            await _db.SaveChangesAsync();

            _storages!.Remove(record.StorageId, out _);
        }
    
        /// <summary>
        /// 获取文件库对象
        /// </summary>
        /// <param name="storageId"></param>
        /// <returns></returns>
        public StorageService GetStorageService(string storageId)
        {
            return _storages![storageId];
        }
    }
}
