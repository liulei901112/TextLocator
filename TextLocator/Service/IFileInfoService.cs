using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Service
{
    /// <summary>
    /// 文件服务
    /// </summary>
    public interface IFileInfoService
    {
        /// <summary>
        /// 获取索引文档
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        Lucene.Net.Documents.Document GetIndexDocument(FileInfo fileInfo);
    }
}
