using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparrowCloud.Models.Union;
using SparrowCloud.Services.Storage;

namespace SparrowCloud.Host.Controllers.Vanilla
{
    [Route("storages/{storageId}/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;

        private readonly UnionContext _dbUnion;

        private readonly StorageManager _manager;

        public FilesController(ILogger<FilesController> logger, UnionContext dbUnion, StorageManager manager)
        {
            _logger = logger;
            _dbUnion = dbUnion;
            _manager = manager;
        }
    }
}
