namespace SparrowCloud.Contracts.RequestModels
{
    /// <summary>
    /// 分页数据请求
    /// </summary>
    public class PageRequest
    {
        public int Page { get; set; }

        public int Limit { get; set; }
    }
}
