using log4net;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextLocator.Core;
using TextLocator.Enums;
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
                    // 文件信息
                    FileInfo fileInfo = new FileInfo(filePath);
                    // 文件名称
                    string fileName = fileInfo.Name;
                    // 文件大小
                    long fileSize = fileInfo.Length;

                    // 压缩包解压
                    builder.Append("名称：" + fileName);
                    builder.Append("　大小：" + FileUtil.GetFileSizeFriendly(fileInfo.Length));
                    builder.Append("　列表：=> \r\n");


                    // 解析列表
                    using (FileStream file = File.OpenRead(filePath))
                    {
                        //设置编码，解决解压文件时中文乱码
                        var archiveEncoding = new ArchiveEncoding();
                        archiveEncoding.Default = System.Text.Encoding.GetEncoding("gbk");
                        var options = new ReaderOptions
                        {
                            ArchiveEncoding = archiveEncoding
                        };
                        using (var archive = ArchiveFactory.Open(file, options))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                if (!entry.IsDirectory)
                                {
                                    builder.Append("┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n");
                                    builder.Append(string.Format("{0},　{1}\r\n", entry.Key, FileUtil.GetFileSizeFriendly(entry.Size)));
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
