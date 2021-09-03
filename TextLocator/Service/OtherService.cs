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
    public class OtherService : IFileInfoService
    {
        public string GetFileContent(string filePath)
        {
            throw new NotImplementedException();
        }

        public Document GetIndexDocument(FileInfo fileInfo)
        {
            throw new NotImplementedException();
        }
    }
}
