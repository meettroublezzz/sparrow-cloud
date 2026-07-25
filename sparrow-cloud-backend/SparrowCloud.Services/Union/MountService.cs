using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
using SparrowCloud.Contracts.MountModels;
using SparrowCloud.Models;
using SparrowCloud.Models.Union;
using SparrowCloud.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Union
{
    public class MountService : ServiceBase
    {
        private readonly ILogger<MountService> _logger;

        private readonly UnionContext _dbUnion;

        private readonly IConfiguration _configuration;

        public MountService(IServiceProvider serviceProvider, ILogger<MountService> logger, UnionContext dbUnion, IConfiguration configuration) : base(serviceProvider)
        {
            _logger = logger;
            _dbUnion = dbUnion;
            _configuration = configuration;
        }

        /// <summary>
        /// 获取默认挂载点
        /// </summary>
        /// <returns></returns>
        private MountModel GetDefaultMount()
        {
            string path = _configuration["SparrowCloud:DefaultPath"]!;

            return new()
            {
                MountId = 0,

                Name = "默认挂载点",
                Path = path,
                Describe = "安装麻雀云盘时由管理员指定的系统数据路径，将作为当前默认挂载点。",
            };
        }
    
        /// <summary>
        /// 实体转DTO
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static MountModel ConvertToModel(UnionMount entity)
        {
            return new()
            {
                MountId = entity.Id,
                Name = entity.Name,
                Path = entity.Path,
                Describe = entity.Describe,
                CreateAt = entity.CreateTime,
            };
        }

        /// <summary>
        /// 查询挂载点
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MountModel>> QueryMountsAsync()
        {
            List<MountModel> result = new();

            var dataset = await _dbUnion.UnionMounts
                .AsNoTracking()
                .ToArrayAsync();

            result.Add(GetDefaultMount());
            
            foreach (var item in dataset)
            {
                result.Add(ConvertToModel(item));
            }

            return result;
        }
    
        /// <summary>
        /// 新增挂载点
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<MountModel> AddMountAsync(MountAddReq req)
        {
            req.NormalizePath();

            if (!Path.Exists(req.Path))
                throw new ServiceException("该挂载点路径对应的目录不存在", 404);

            // 是否有完整权限
            if(!DirectoryHelper.CheckFullControl(req.Path))
                throw new ServiceException("麻雀云盘不具备该路径的完整访问权限", 403);

            UnionMount mount = new()
            {
                Id = default,

                Name = req.Name,
                Path = req.Path,
                Describe = req.Describe,
            };

            _dbUnion.UnionMounts .Add(mount);

            await _dbUnion.SaveChangesAsync();

            return ConvertToModel(mount);
        }
    
        /// <summary>
        /// 编辑挂载点
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<MountModel> EditMountAsync(int mountId, MountEditReq req)
        {
            req.NormalizePath();

            if (mountId == 0)
                throw new ServiceException("不能修改默认挂载点");

            if (!Path.Exists(req.Path))
                throw new ServiceException("该挂载点路径对应的目录不存在", 404);

            var mount = await _dbUnion.UnionMounts.FindAsync(mountId);

            if (mount == null)
                throw new ServiceException("挂载点ID不存在，请检查", 404);

            mount.Name = req.Name;
            mount.Path = req.Path;
            mount.Describe = req.Describe;
            mount.UpdateAt = EntityBase.NowTicks;

            await _dbUnion.SaveChangesAsync();

            return ConvertToModel(mount);
        }

        /// <summary>
        /// 删除挂载点（仅仅删除记录，不影响任何文件库）
        /// </summary>
        /// <param name="mountId"></param>
        /// <returns></returns>
        public async Task DelMountAsync(int mountId)
        {
            int rows = await _dbUnion.UnionMounts
                .Where(e => e.Id == mountId)
                .ExecuteDeleteAsync();

            if (rows != 1)
                throw new Exception($"删除操作出现意料之外的错误：rows={rows}");
        }

        /// <summary>
        /// 查询用户常见目录及权限
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<DirectoryEntry> GetDirectoryEntry()
        {
            return DirectoryHelper.GetUserDirectories();
        }
        /// <summary>
        /// 查询某路径下的目录树
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static IEnumerable<DirectoryTreeNode> GetDirectoryTree(string rootPath)
        {
            return DirectoryHelper.GetDirectoryTree(rootPath, true);
        }
    }
}
