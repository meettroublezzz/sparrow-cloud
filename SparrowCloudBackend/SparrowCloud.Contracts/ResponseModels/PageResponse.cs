using System;
using System.Collections;

namespace SparrowCloud.Contracts.ResponseModels
{
    /// <summary>
    /// 分页数据响应
    /// </summary>
    public class PageResponse<TItemType> : BaseResponse
    {
        /// <summary>
        /// 数据载荷
        /// </summary>
        public required IEnumerable<TItemType> Payload { get; set; }

        /// <summary>
        /// 数据总数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 成功的响应
        /// </summary>
        /// <returns></returns>
        public static PageResponse<TItemType> Success(int total, IEnumerable<TItemType> dataset)
        {
            return new PageResponse<TItemType>()
            {
                Code = 0,
                Message = "success",
                Succeed = true,
                Total = total,
                Payload = dataset,
            };
        }

        /// <summary>
        /// 成功的响应
        /// </summary>
        /// <returns></returns>
        public static PageResponse<TItemType> Success((int total, IEnumerable<TItemType> dataset) result)
        {
            return Success(result.total, result.dataset);
        }

        /// <summary>
        /// 失败的响应（不建议默认）
        /// </summary>
        /// <returns></returns>
        public static new PageResponse<TItemType> Fail(int code = -1, string msg = "fail")
        {
            return new PageResponse<TItemType>()
            {
                Code = code,
                Message = msg,
                Succeed = false,
                Total = -1,
                Payload = [],
            };
        }
    }
}
