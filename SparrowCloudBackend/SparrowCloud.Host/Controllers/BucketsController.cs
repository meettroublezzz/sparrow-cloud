using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Algorithm;
using SparrowCloud.Models.Intermediate;
using SparrowCloud.Services;
using SparrowCloud.Services.Storage;

namespace SparrowCloud.Host.Controllers
{
    [Route("buckets")]
    [ApiController]
    public class BucketsController : ControllerBase
    {
        private readonly ILogger<BucketsController> _logger;

        private readonly IntermediateContext _dbInterm;

        private readonly StorageManager _manager;

        public BucketsController(ILogger<BucketsController> logger, IntermediateContext dbInterm, StorageManager manager)
        {
            _logger = logger;
            _dbInterm = dbInterm;
            _manager = manager;
        }

        /// <summary>
        /// 桶文件上传
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="bucketName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("{storageId}/{bucketName}")]
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
                _logger.LogWarning(se, "文件上传异常");

                return StatusCode(se.Code, se.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件上传失败");

                return StatusCode(StatusCodes.Status500InternalServerError, "文件上传失败");
            }
        }

        /// <summary>
        /// 桶文件下载
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="bucketName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpGet("{storageId}/{bucketName}/{objectName}")]
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
                _logger.LogWarning(se, "文件获取异常");

                return StatusCode(se.Code, se.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件获取失败");

                return StatusCode(StatusCodes.Status500InternalServerError, "文件获取失败");
            }
        }

        /// <summary>
        /// 桶文件删除
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        [HttpDelete("{storageId}/{bucketName}/{objectName}")]
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
                _logger.LogWarning(se, "文件删除异常");

                return StatusCode(se.Code, se.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件删除失败");

                return StatusCode(StatusCodes.Status500InternalServerError, "文件删除失败");
            }
        }
    }
}
