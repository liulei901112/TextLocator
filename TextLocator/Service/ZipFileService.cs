using log4net;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextLocator.Core;
using TextLocator.Exceptions;
using TextLocator.Factory;
using TextLocator.Util;

namespace TextLocator.Service
{
    /// <summary>
    /// 压缩包文件服务
    /// </summary>
    public class ZipFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static object locker = new object();

        public string GetFileContent(string filePath)
        {
            // 内容
            StringBuilder builder = new StringBuilder();
            lock (locker)
            {
                try
                {
                    // 压缩包解压
                    builder.Append("名称：" + filePath.Substring(filePath.LastIndexOf("\\") + 1));
                    builder.Append("　大小：" + FileUtil.GetFileSizeFriendly(new FileInfo(filePath).Length) + " =>\r\n");

                    builder.Append("　列表：=>\r\n");
                    using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var archive = ArchiveFactory.Open(file)) {
                            foreach (var entry in archive.Entries)
                            {
                                if (!entry.IsDirectory)
                                {
                                    builder.Append(String.Format("　　{0},　{1}\r\n", entry.Key, FileUtil.GetFileSizeFriendly(entry.Size)));                                 
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(filePath + " -> " + ex.Message, ex);
                }
            }
            return builder.ToString();
        }
    }
}
