using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Exceptions
{
    /// <summary>
    /// 文件太大
    /// </summary>
    public class FileBigSizeException : IOException
    {
        public FileBigSizeException(string message) : base(message)
        {
        }
    }
}
