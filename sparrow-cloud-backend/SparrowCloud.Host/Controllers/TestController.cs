using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SparrowCloud.Models.Intermediate;
using SparrowCloud.Models.Storage;
using SparrowCloud.Services;
using SparrowCloud.Services.Storage;

namespace SparrowCloud.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<ServiceBase> _logger;

        private readonly IntermediateContext _dbInterm;

        private readonly StorageManager _manager;


        public TestController(ILogger<ServiceBase> logger, IntermediateContext dbInterm, StorageManager manager)
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

        [HttpPost("task/scan-files")]
        public async Task ScanFilesAsync([FromForm] string storageId)
        {
            await _manager.ScanFilesAsync(UserId, storageId);

            var storage = StorageManager.GetStorageService(UserId, storageId);

            await storage.TestAsync();
        }
    }
}
