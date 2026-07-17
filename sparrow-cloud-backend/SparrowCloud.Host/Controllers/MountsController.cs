using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparrowCloud.Contracts.MountModels;
using SparrowCloud.Contracts.ResponseModels;
using SparrowCloud.Host.Controllers.Vanilla;
using SparrowCloud.Models.Union;
using SparrowCloud.Services;
using SparrowCloud.Services.Storage;
using SparrowCloud.Services.Union;

namespace SparrowCloud.Host.Controllers
{
    /// <summary>
    /// 挂载点管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MountsController : ControllerBase
    {
        private readonly ILogger<MountsController> _logger;

        private readonly MountService _mountService;

        public MountsController(ILogger<MountsController> logger, MountService mountService)
        {
            _logger = logger;
            _mountService = mountService;
        }

        [HttpGet("metadata")]
        public async Task<BaseResponse<MetadataModel>> GetMetadataAsync([FromForm] string? path)
        {
            try
            {
                var result = _mountService.GetMetadata(path);

                return BaseResponse<MetadataModel>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "查询元数据：");

                return BaseResponse<MetadataModel>.Fail(se.Code, se.Message);
            }
        }

        [HttpGet]
        public async Task<BaseResponse<IEnumerable<MountModel>>> QueryMountsAsync()
        {
            try
            {
                var result = await _mountService.QueryMountsAsync();

                return BaseResponse<IEnumerable<MountModel>>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "查询挂载点：");

                return BaseResponse<IEnumerable<MountModel>>.Fail(se.Code, se.Message);
            }
        }

        [HttpPost]
        public async Task<BaseResponse<MountModel>> AddMountAsync([FromBody] MountAddReq req)
        {
            try
            {
                var result = await _mountService.AddMountAsync(req);

                return BaseResponse<MountModel>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "新增挂载点：");

                return BaseResponse<MountModel>.Fail(se.Code, se.Message);
            }
        }

        [HttpPut("{mountId}")]
        public async Task<BaseResponse<MountModel>> EditMountAsync([FromRoute]int mountId, [FromBody]MountEditReq req)
        {
            try
            {
                var result = await _mountService.EditMountAsync(mountId, req);

                return BaseResponse<MountModel>.Success(result);
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "修改挂载点：");

                return BaseResponse<MountModel>.Fail(se.Code, se.Message);
            }
        }

        [HttpDelete("{mountId}")]
        public async Task<BaseResponse> DelMountAsync([FromRoute] int mountId)
        {
            try
            {
                await _mountService.DelMountAsync(mountId);

                return BaseResponse.Success();
            }
            catch (ServiceException se)
            {
                _logger.LogWarning(se, "删除挂载点：");

                return BaseResponse.Fail(se.Code, se.Message);
            }
        }
    }
}
