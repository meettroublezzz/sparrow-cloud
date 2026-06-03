using Microsoft.EntityFrameworkCore;
using SparrowCloud.Models;
using SparrowCloud.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Storage
{
    /*
     * 放置其他业务代码
     */
    public partial class StorageService
    {
        public async Task TestAsync()
        {
            using var db = GetStorageContext();

            var file = await db.StorageFiles
                .Include(e => e.Information)
                .FirstAsync();

            if (file.Information == null)
            {
                file.Information = new()
                {
                    Id = default,
                    Title = "first",
                };

                await db.SaveChangesAsync();

                Console.WriteLine("首次创建 Information");

                return;
            }

            var references = file.Information.References;

            references.Add(new()
            {
                Id = EntityBase.GenerateGuid(),
                Type = ReferenceType.Source,
                Title = Random.Shared.Next().ToString(),
            });

            if (references.Count == 5)
            {
                references.Clear();
            }

            Console.WriteLine($"r-count -> {references.Count}");
            foreach (var item in references)
            {
                Console.WriteLine($"item -> {item.Id}: {item.Title}");
            }
            Console.WriteLine();

            file.Information.References = references;

            await db.SaveChangesAsync();

            Console.WriteLine("更新 Information References");
        }
    }
}
