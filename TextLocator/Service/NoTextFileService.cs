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
    /// 无文本文件服务
    /// </summary>
    public class NoTextFileService : IFileInfoService
    {
        public string GetFileContent(string filePath)
        {
            return filePath;
        }
    }
}
