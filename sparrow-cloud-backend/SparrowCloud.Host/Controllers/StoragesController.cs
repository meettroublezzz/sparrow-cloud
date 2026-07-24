using Microsoft.AspNetCore.Mvc;
using SparrowCloud.Contracts.MountModels;
using SparrowCloud.Contracts.ResponseModels;
using SparrowCloud.Contracts.StorageModels;
using SparrowCloud.Services;
using SparrowCloud.Services.Storage;
using SparrowCloud.Services.Union;

namespace SparrowCloud.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoragesController : ControllerBase
    {
        private readonly ILogger<StoragesController> _logger;

        private readonly StorageManager _storageManager;

        public StoragesController(ILogger<StoragesController> logger, StorageManager storageManager)
        {
            _logger = logger;
            _storageManager = storageManager;
        }

        [HttpGet]
        public async Task<BaseResponse<IEnumerable<StorageModel>>> QueryStorageAsync()
        {
            try
            {
                var result = await _storageManager.QueryStorageAsync(TestController.UserId);

                return BaseResponse<IEnumerable<StorageModel>>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "查询文件库：");

                return BaseResponse<IEnumerable<StorageModel>>.Fail(se.Code, se.Message);
            }
        }

        [HttpPost]
        public async Task<BaseResponse<StorageModel>> CreateOrAttachAsync([FromQuery] StorageAddType type, [FromBody] StorageAddReq req)
        {
            try
            {
                StorageModel result;

                switch (type)
                {
                    case StorageAddType.Create:
                        result = await _storageManager.CreateStorageAsync(TestController.UserId, "nickname", req);
                        break;

                    case StorageAddType.Attach:
                        result = await _storageManager.AttachStorageAsync(TestController.UserId, "nickname", req);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return BaseResponse<StorageModel>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, $"创建或附加文件库 {type}：");

                return BaseResponse<StorageModel>.Fail(se.Code, se.Message);
            }
        }

        [HttpPut("{storageId}")]
        public async Task<BaseResponse<StorageModel>> EditStorageAsync([FromRoute] string storageId, [FromBody] StorageEditReq req)
        {
            try
            {
                var result = await _storageManager.EditStorageAsync(TestController.UserId, storageId, req);

                return BaseResponse<StorageModel>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, $"修改文件库信息：");

                return BaseResponse<StorageModel>.Fail(se.Code, se.Message);
            }
        }

        [HttpPatch("{storageId}")]
        public async Task<BaseResponse<StorageModel>> PatchStorageAsync([FromRoute] string storageId, [FromForm] bool? remove = null)
        {
            try
            {
                var result = await _storageManager.PatchStorageAsync(TestController.UserId, storageId, remove);

                return BaseResponse<StorageModel>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, $"修改文件库状态：");

                return BaseResponse<StorageModel>.Fail(se.Code, se.Message);
            }
        }

        [HttpDelete("{storageId}")]
        public async Task<BaseResponse> RemoveOrDeleteStorageAsync([FromRoute] string storageId, [FromQuery] bool isDelete = false)
        {
            try
            {
                await _storageManager.RemoveOrDeleteStorageAsync(TestController.UserId, storageId, isDelete);

                return BaseResponse.Success();
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, $"移除或删除文件库 ：{isDelete}");

                return BaseResponse.Fail(se.Code, se.Message);
            }
        }
    }
}
