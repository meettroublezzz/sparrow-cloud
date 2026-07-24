using Codeuctivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using SparrowCloud.Contracts.StorageModels;
using SparrowCloud.Models;
using SparrowCloud.Models.Union;
using System.Collections.Concurrent;

namespace SparrowCloud.Services.Storage
{
    public class StorageManager
    {
        #region 相关约定名称
        /// <summary>
        /// 数据仓库名称
        /// </summary>
        public const string StandaloneDirectoryName = @".__SparrowCloud__DWH__Standalone__";

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
        public static async Task InitAsync(UnionContext db, UnionStorage[]? dataset = null)
        {
            /*
             * 从数据库读取当前所有挂载的文件库，初始化管理器；
             */

            if (dataset == null)
            {
                dataset = db.UnionStorages
                    .Where(e => e.DeletedAt == null && e.Missing == null && e.Damaged == null)
                    .ToArray();
            }

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

                string key = $"{item.UserId}+{metadata.StorageGuid}";

                // 实例化文件库
                StorageService storage = new(rootPath, metadata);

                _storages.TryAdd(key, storage);
            }

            db.SaveChanges();
        }

        /// <summary>
        /// 实体转DTO
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static async Task<StorageModel> ConvertToModel(string uid, UnionStorage entity)
        {
            string key = $"{uid}+{entity.StorageId}";

            // 是否就绪
            string? know = null;

            BaseInfoModel? info = null;
            StorageMetadata? metadata = null;
            long size = -1;

            if (_storages.TryGetValue(key, out var storage))
            {
                info = await storage.GetStorageBaseInfoAsync(entity);
                size = await storage.GetStorageSizeAsync();
                metadata = storage.Metadata;
            }
            else
            {
                know = "--???--";
            }

            return new()
            {
                StorageId = entity.StorageId,
                Sequence = entity.Sequence,

                CoverUrl = know != null ? know! : info!.CoverUrl!,
                Name = entity.Name,
                DirName = Path.GetFileName(entity.RootPath)!,
                FullPath = entity.RootPath,

                Missing = entity.Missing,
                LastScan = entity.LastScan,
                Damaged = entity.Damaged,

                CreatedAt = entity.CreateTime,
                UpdatedAt = entity.UpdateTime,
                LastAccessAt = entity.LastAccessAt,

                DeletedAt = entity.DeletedAt,

                Size = size,
                Describe = entity.Describe,
                Birthday = know != null ? default : metadata!.CreateAt,

                Ready = know == null,
            };
        }

        /// <summary>
        /// 查询文件库信息
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<StorageModel>> QueryStorageAsync(string uid)
        {
            List<StorageModel> result = new();

            var dataset = await _db.UnionStorages
                .AsNoTracking()
                .Where(e => e.UserId == uid)
                .OrderByDescending(e => e.Sequence)
                .ToArrayAsync();

            foreach (var storage in dataset)
            {
                var tmp = await ConvertToModel(uid, storage);

                result.Add(tmp);
            }

            return result;
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
        public async Task<UnionStorage> CreateOrAttachAsync(string uid, string creator, string name, string desc, string path)
        {
            string rootPath = Path.TrimEndingDirectorySeparator(path);

            if (!Directory.Exists(rootPath))
                throw new ServiceException("此文件库路径实际不存在！", 404);

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
                Describe = desc,

                UserId = uid,
                StorageId = metadata.StorageGuid,

                RootPath = rootPath,

                Sequence = seq + EntityBase.SequenceStepSize,
            };

            // 添加入库
            _db.UnionStorages.Add(model);
            _db.SaveChanges();

            #region 尝试激活文件库，使其就绪
            await InitAsync(_db, [model]);

            string key = $"{model.UserId}+{model.StorageId}";

            if (!_storages.TryGetValue(key, out var storage))
                throw new ServiceException("文件库未就绪，请检查状态", 10);
            #endregion

            return model;
        }

        /// <summary>
        /// 创建文件库
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<StorageModel> CreateStorageAsync(string uid, string creator, StorageAddReq req)
        {
            /*
             * 创建文件库
             *      可能从挂载点路径出发，需要根据 UID/文件库名称 得到实际目录路径，
             *      如果前端没有传递 DirName 则依赖Name值（并处理名称合法性问题）
             */

            string dirName = string.IsNullOrWhiteSpace(req.DirName) ? req.Name : req.DirName;

            // 处理为合法性目录名称
            dirName = dirName.SanitizeFilename();

            string fullPath = Path.Combine(req.FullPath, uid, dirName);

            if (Directory.Exists(fullPath))
                throw new ServiceException($"该路径下已存在名为'{dirName}'的目录，无法创建！", 409);

            Directory.CreateDirectory(fullPath);

            var model = await CreateOrAttachAsync(uid, creator, req.Name, req.Describe, fullPath);

            return await ConvertToModel(uid, model);
        }
        
        /// <summary>
        /// 附加文件库
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="creator"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<StorageModel> AttachStorageAsync(string uid, string creator, StorageAddReq req)
        {
            /*
             * 前端必须传递 FullPath
             *      直接根据 FullPath 附加文件库即可，不一定需要存在数据仓库
             *          如果存在数据仓库则视为标准文件库，尝试附加；
             *          如果不存在则视为普通目录，初始化数据仓库后附加；
             */

            string fullPath = req.FullPath;

            string dirName = Path.GetFileName(fullPath)!;

            if (!Directory.Exists(fullPath))
                throw new ServiceException($"该路径下不存在名为'{dirName}'的目录，无法附加！", 404);

            var model = await CreateOrAttachAsync(uid, creator, req.Name, req.Describe, fullPath);

            return await ConvertToModel(uid, model);
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
        /// 修改文件库信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<StorageModel> EditStorageAsync(string uid, string storageId, StorageEditReq req)
        {
            var record = await _db.UnionStorages
                .Where(e => e.UserId == uid && e.StorageId == storageId)
                .SingleAsync();

            record.Name = req.Name;
            record.Describe = req.Describe;
            record.UpdateAt = EntityBase.NowTicks;

            await _db.SaveChangesAsync();

            return await ConvertToModel(uid, record);
        }

        /// <summary>
        /// 移除 or 删除 文件库（不会删除实际目录或文件）
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="storageId"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        public async Task RemoveOrDeleteStorageAsync(string uid, string storageId, bool isDelete = false)
        {
            var record = await _db.UnionStorages
                .Where(e => e.UserId == uid && e.StorageId == storageId)
                .SingleAsync();

            if (isDelete)
            {
                _db.UnionStorages.Remove(record);
            }
            else
            {
                record.DeletedAt = DateTime.Now;
                record.LastScan = null;
                record.Missing = null;
                record.Damaged = null;
            }
            
            await _db.SaveChangesAsync();

            _storages.Remove($"{uid}+{storageId}", out _);
        }
        
        /// <summary>
        /// 修改文件库状态
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="storageId"></param>
        /// <returns></returns>
        public async Task<StorageModel> PatchStorageAsync(string uid, string storageId, bool? remove = null)
        {
            var record = await _db.UnionStorages
                .Where(e => e.UserId == uid && e.StorageId == storageId)
                .SingleAsync();

            // 取消移除状态
            if (remove != null && !((bool)remove))
            {
                record.DeletedAt = null;
                record.UpdateAt = EntityBase.NowTicks;
            }

            await _db.SaveChangesAsync();

            #region 尝试激活文件库，使其就绪
            string key = $"{record.UserId}+{record.StorageId}";

            if (!_storages.ContainsKey(key) && record.DeletedAt == null)
            {
                _logger.LogDebug("修改状态后 -> 尝试激活文件库，使其就绪");

                await InitAsync(_db, [record]);

                if (!_storages.TryGetValue(key, out var storage))
                    throw new ServiceException("文件库未就绪，请检查状态", 10);
            }
            #endregion

            return await ConvertToModel(uid, record);
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
