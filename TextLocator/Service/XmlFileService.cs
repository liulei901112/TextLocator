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
    /// Xml文件文服务
    /// </summary>
    public class XmlFileService : IFileInfoService
    {
        public string GetFileContent(string filePath)
        {
            return filePath;
        }
    }
}
