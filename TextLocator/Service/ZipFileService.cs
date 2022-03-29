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

                    // 解析文件内容 && 文件大小
                    if (AppConst.IS_PARSE_ZIP_CONTENT && fileSize <= AppConst.ZIP_FILE_SIZE_LIMIT)
                    {
                        // 获取文件信息，判断压缩包大小。大于限制大小的同样不解析文件内容
                        string unzipPath = Path.Combine(AppConst.APP_TEMP_DIR, fileName);

                        // 判断文件夹是否存在，不存在创建
                        if (!Directory.Exists(unzipPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(unzipPath);
                            }
                            catch (Exception ex)
                            {
                                log.Error("解压文件夹创建失败：" + ex.Message, ex);
                            }
                        }

                        builder.Append(GetContent(filePath, unzipPath));
                    }
                    else
                    {
                        builder.Append(GetContent(filePath, null));
                    }
                }
                catch (Exception ex)
                {
                    log.Error(filePath + " -> " + ex.Message, ex);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 获取内容
        /// </summary>
        /// <param name="filePath">压缩文件路径</param>
        /// <param name="unzipPath">解压路径</param>
        /// <param name="isParseContent">是否解析文件内容</param>
        /// <returns></returns>
        private string GetContent(string filePath, string unzipPath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var archive = ArchiveFactory.Open(file))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            builder.Append("┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n");
                            builder.Append(string.Format("{0},　{1}\r\n", entry.Key, FileUtil.GetFileSizeFriendly(entry.Size)));
                            // 解压路径不为空，代表需要解压并解析
                            if (!string.IsNullOrEmpty(unzipPath))
                            {
                                // 获取文件类型
                                FileType fileType = FileTypeUtil.GetFileType(entry.Key);
                                // 文件后缀
                                string fileExt = Path.GetExtension(entry.Key);
                                if (!string.IsNullOrEmpty(fileExt))
                                {
                                    fileExt = fileExt.Substring(1);
                                }
                                // 支持的文件后缀集
                                string fileExts = FileTypeUtil.GetFileTypeExts();
                                // 不是压缩包 && 文件后缀在可解析范围
                                if (fileType != FileType.压缩包 && fileExts.Contains(fileExt))
                                {
                                    // 判断文件是否支持解析，不支持解析的文件无需解压
                                    entry.WriteToDirectory(unzipPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });

                                    // 获取文件信息，判断压缩包大小。大于限制大小的同样不解析文件内容
                                    string unzipFile = Path.Combine(unzipPath, entry.Key);
                                    // 解析文件内容
                                    builder.Append(FileInfoServiceFactory.GetFileContent(unzipFile) + "\r\n");
                                }
                            }
                        }
                    }
                }
            }
            return builder.ToString();
        }
    }
}
