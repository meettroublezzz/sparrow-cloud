using Microsoft.EntityFrameworkCore;
using SparrowCloud.Models;
using SparrowCloud.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SparrowCloud.Services.Storage
{
    /*
    * 标签系统
    */
    public partial class StorageService
    {
        /// <summary>
        /// 查询现有标签
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<StorageTag>> QueryTagsAsync()
        {
            using var db = GetStorageContext();

            return await db.StorageTags
                .AsNoTracking()
                //.Where()
                .ToArrayAsync();
        }

        /// <summary>
        /// 创建标签
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pid"></param>
        /// <param name="color"></param>
        /// <returns>新标签ID</returns>
        public async Task<int> CreateTagAsync(string name, int? pid, string? color = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ServiceException("标签名称不能为空");
            }

            using var db = GetStorageContext();

            bool any = db.StorageTags.Any(e => e.Name == name);

            if (any)
            {
                throw new ServiceException("标签名称已存在");
            }

            var tag = new StorageTag()
            {
                Id = default,

                Name = name,
                Color = color,

                TagParentId = pid,
            };

            db.StorageTags.Add(tag);

            await db.SaveChangesAsync();

            return tag.Id;
        }

        /// <summary>
        /// 修改某个标签
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="pid"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public async Task AlterTagAsync(int id, string name, int? pid, string? color = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ServiceException("标签名称不能为空");
            }

            using var db = GetStorageContext();

            bool any = db.StorageTags.Any(e => e.Name == name);

            if (any)
            {
                throw new ServiceException("标签名称已存在");
            }

            var tag = await db.StorageTags.SingleAsync(e => e.Id == id);

            tag.Name = name;
            tag.Color = color;
            tag.TagParentId = pid;
            tag.UpdateAt = EntityBase.NowTicks;

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// 删除某个标签
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteTagAsync(int id)
        {
            using var db = GetStorageContext();

            var tran = await db.Database.BeginTransactionAsync();

            try
            {
                var tag = await db.StorageTags.SingleAsync(e => e.Id == id);

                // 如果被删除的是父标签，则把子标签移动到默认下面
                if (tag.TagParentId == null)
                {
                    await db.StorageTags
                        .Where(e => e.TagParentId == id)
                        .ExecuteUpdateAsync(e => e
                            .SetProperty(s => s.TagParentId, 1)
                            .SetProperty(s => s.UpdateAt, EntityBase.NowTicks)
                        );
                }

                // 删除标签
                db.StorageTags.Remove(tag);

                await db.SaveChangesAsync();

                await tran.CommitAsync();
            }
            catch
            {
                await tran.RollbackAsync();

                throw new ServiceException("删除标签失败", 500);
            }
        }

        /// <summary>
        /// 给文件绑定标签（覆盖式）
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="tagIds"></param>
        /// <returns></returns>
        public async Task ReplaceFileTagsAsync(long fileId, IEnumerable<int> tagIds)
        {
            using var db = GetStorageContext();

            var tran = await db.Database.BeginTransactionAsync();

            try
            {
                // 删掉此文件的全部绑定标签
                await db.StorageTagFile
                .Where(e => e.FileId == fileId)
                .ExecuteDeleteAsync();

                // 覆盖式回写
                foreach (var tid in tagIds)
                {
                    db.StorageTagFile.Add(new StorageTagFile()
                    {
                        Id = default,

                        FileId = fileId,
                        TagId = tid,
                    });
                }

                await db.SaveChangesAsync();

                await tran.CommitAsync();
            }
            catch
            {
                await tran.RollbackAsync();

                throw new ServiceException("绑定标签失败", 500);
            }
        }
    }
}
