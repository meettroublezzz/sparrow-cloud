using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Algorithm;
using SparrowCloud.Models.Union;
using SparrowCloud.Services;
using SparrowCloud.Services.Storage;

namespace SparrowCloud.Host.Controllers.Vanilla
{
    [Route("storages/{storageId}/buckets")]
    [ApiController]
    public class BucketsController : ControllerBase
    {
        private readonly ILogger<BucketsController> _logger;

        private readonly UnionContext _dbUnion;

        private readonly StorageManager _manager;

        public BucketsController(ILogger<BucketsController> logger, UnionContext dbUnion, StorageManager manager)
        {
            _logger = logger;
            _dbUnion = dbUnion;
            _manager = manager;
        }

        /// <summary>
        /// 桶文件上传
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("{bucketName}")]
        public async Task<IActionResult> UploadAsync([FromRoute] string storageId, [FromRoute] string bucketName, IFormFile file)
        {
            try
            {
                var storage = StorageManager.GetStorageService(TestController.UserId, storageId);

                long length = file.Length;
                using var stream = file.OpenReadStream();

                var result = await storage.UploadBucketFileAsync(bucketName, length, stream, file.FileName);

                return Content(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "桶文件上传异常");

                return StatusCode(se.Code, se.Message);
            }
        }

        /// <summary>
        /// 桶文件下载
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpGet("{bucketName}/{objectName}")]
        public async Task<IActionResult> DownAsync([FromRoute] string storageId, [FromRoute] string bucketName, [FromRoute] string objectName)
        {
            try
            {
                var storage = StorageManager.GetStorageService(TestController.UserId, storageId);

                var (stream, contentType) = await storage.DownBucketFileAsync(bucketName, objectName);

                return File(stream, contentType);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "桶文件获取异常");

                return StatusCode(se.Code, se.Message);
            }
        }

        /// <summary>
        /// 桶文件删除
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        [HttpDelete("{bucketName}/{objectName}")]
        public async Task<IActionResult> DelAsync([FromRoute] string storageId, [FromRoute] string bucketName, [FromRoute] string objectName)
        {
            try
            {
                var storage = StorageManager.GetStorageService(TestController.UserId, storageId);

                await storage.DelBucketFileAsync(bucketName, objectName);

                return Ok();
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "桶文件删除异常");

                return StatusCode(se.Code, se.Message);
            }
        }
    }
}
