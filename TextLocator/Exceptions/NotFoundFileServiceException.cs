using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Exceptions
{
    /// <summary>
    /// 未找到文件服务
    /// </summary>
    public class NotFoundFileServiceException : Exception
    {        public NotFoundFileServiceException(string message) : base(message)
        {
        }
    }
}
