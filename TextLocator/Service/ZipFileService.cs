using log4net;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextLocator.Core;
using TextLocator.Exceptions;
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
                    FileInfo fileInfo = new FileInfo(filePath);
                    System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                    builder.Append("文件名称：" + info.FileName.Substring(info.FileName.LastIndexOf("\\") + 1));
                    builder.Append("　文件大小：" + FileUtil.GetFileSizeFriendly(fileInfo.Length) + "=>\r\n");

                    builder.Append("　文件列表：->\r\n");
                    using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var archive = ArchiveFactory.Open(file);
                        foreach (var entry in archive.Entries) {
                            if (!entry.IsDirectory)
                            {
                                builder.Append(String.Format("　　文件名：{0},　大小：{1}\r\n", entry.Key, FileUtil.GetFileSizeFriendly(entry.Size)));
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
