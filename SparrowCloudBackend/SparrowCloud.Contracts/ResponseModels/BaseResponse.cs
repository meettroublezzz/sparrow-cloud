namespace SparrowCloud.Contracts.ResponseModels
{
    /// <summary>
    /// 标准响应
    /// </summary>
    public class BaseResponse
    {
        /*
         * 此标准响应适用于 HttpCode=200 的情况，
         * 不是 200 OK 的响应，则根据不同情况具体处理。
         */

        /// <summary>
        /// 错误码：0表达成功、正常响应
        /// </summary>
        public required int Code { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// 是否成功（方便前端处理）
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 成功的响应
        /// </summary>
        /// <returns></returns>
        public static BaseResponse Success()
        {
            return new BaseResponse()
            {
                Code = 0,
                Message = "success",
                Succeed = true,
            };
        }

        /// <summary>
        /// 失败的响应（不建议默认）
        /// </summary>
        /// <returns></returns>
        public static BaseResponse Fail(int code = -1, string msg = "fail")
        {
            return new BaseResponse()
            {
                Code = code,
                Message = msg,
                Succeed = false,
            };
        }
    }

    /// <summary>
    /// 带数据响应
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class BaseResponse<TResult> : BaseResponse
    {
        /// <summary>
        /// 数据载荷
        /// </summary>
        public TResult? Payload { get; set; }

        /// <summary>
        /// 成功的响应
        /// </summary>
        /// <returns></returns>
        public static BaseResponse<TResult> Success(TResult result)
        {
            return new BaseResponse<TResult>()
            {
                Code = 0,
                Message = "success",
                Succeed = true,
                Payload = result
            };
        }

        /// <summary>
        /// 失败的响应（不建议默认）
        /// </summary>
        /// <returns></returns>
        public static new BaseResponse<TResult> Fail(int code = -1, string msg = "fail")
        {
            return new BaseResponse<TResult>()
            {
                Code = code,
                Message = msg,
                Succeed = false,
                Payload = default,
            };
        }
    }

    
}
