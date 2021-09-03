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
    /// 文本服务
    /// </summary>
    public class TxtService : IFileInfoService
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
