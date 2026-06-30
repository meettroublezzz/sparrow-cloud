using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SparrowCloud.Models.Storage;
using SparrowCloud.Models.Union;
using SparrowCloud.Services;
using SparrowCloud.Services.Storage;

namespace SparrowCloud.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<ServiceBase> _logger;

        private readonly UnionContext _dbInterm;

        private readonly StorageManager _manager;


        public TestController(ILogger<ServiceBase> logger, UnionContext dbInterm, StorageManager manager)
        {
            _logger = logger;
            _dbInterm = dbInterm;
            _manager = manager;
        }

        public const string UserId = "zyq";

        [HttpGet]
        public async Task<dynamic> Test()
        {
            var dataset = await _manager.QueryStorageAsync(UserId);

            return new
            {
                dataset,
            };
        }

        [HttpPost]
        public async Task<dynamic> Testa([FromForm] string path)
        {
            return await _manager.CreateOrAttachAsync(UserId, UserId, path);
        }

        [HttpDelete]
        public async Task Del([FromForm] string id)
        {
            await _manager.RemoveStorageAsync(UserId, id);
        }

        [HttpDelete("DeleteFileByIdAsync")]
        public async Task DeleteFileByIdAsync([FromForm] string storageId, [FromForm] long fileId)
        {
            var storage = StorageManager.GetStorageService(UserId, storageId);

            await storage.DeleteFileByIdAsync(fileId);
        }

        [HttpPost("CreateTagAsync")]
        public async Task CreateTagAsync([FromForm] string storageId, [FromForm] string name, [FromForm] int? pid)
        {
            var storage = StorageManager.GetStorageService(UserId, storageId);

            await storage.CreateTagAsync(name, pid);
        }
        [HttpGet("QueryTagsAsync")]
        public async Task<object> QueryTagsAsync([FromQuery] string storageId)
        {
            var storage = StorageManager.GetStorageService(UserId, storageId);

            return await storage.QueryTagsAsync();
        }
        [HttpDelete("DeleteTagAsync")]
        public async Task DeleteTagAsync([FromForm] string storageId, [FromForm] int tid)
        {
            var storage = StorageManager.GetStorageService(UserId, storageId);

            await storage.DeleteTagAsync(tid);
        }

        [HttpPut("ReplaceFileTagsAsync")]
        public async Task ReplaceFileTagsAsync([FromQuery] string storageId, [FromQuery] long fileId, [FromBody] int[] tagIds)
        {
            var storage = StorageManager.GetStorageService(UserId, storageId);

            await storage.ReplaceFileTagsAsync(fileId, tagIds);
        }

        [HttpPost("task/scan-files")]
        public async Task ScanFilesAsync([FromForm] string storageId)
        {
            await _manager.ScanFilesAsync(UserId, storageId);

            var storage = StorageManager.GetStorageService(UserId, storageId);

            await storage.TestAsync();
        }
    }
}
