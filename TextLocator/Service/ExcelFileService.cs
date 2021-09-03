using log4net;
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
    /// Excel文件服务
    /// </summary>
    public class ExcelFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetFileContent(string filePath)
        {
            return filePath;
        }
    }
}
