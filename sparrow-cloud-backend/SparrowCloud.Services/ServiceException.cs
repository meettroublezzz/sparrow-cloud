using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services
{
    public class ServiceException : Exception
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; private set; }

        public ServiceException(string message, int code = -1) : base(message)
        {
            Code = code;
        }
    }
}
