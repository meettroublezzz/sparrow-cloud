using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Utils
{
    public static class HttpHelper
    {
        private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public static FileExtensionContentTypeProvider ContentTypeProvider { get => _contentTypeProvider; }

        /// <summary>
        /// 根据扩展名得到 ContentType
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetContentTypeByExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException();

            if (!extension.StartsWith('.'))
                extension = $".{extension}";

            if (!_contentTypeProvider.TryGetContentType(extension, out string contentType))
            {
                // 找不到后缀映射时，兜底为通用二进制流
                contentType = "application/octet-stream";
            }

            return contentType;
        }
    }
}
