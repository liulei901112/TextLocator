using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Service
{
    /// <summary>
    /// 其他类型文件服务
    /// </summary>
    public class OtherFileService : IFileInfoService
    {
        public string GetFileContent(string filePath)
        {
            return filePath;
        }
    }
}
