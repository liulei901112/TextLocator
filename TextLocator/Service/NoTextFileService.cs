using log4net;
using System;
using System.Text;

namespace TextLocator.Service
{
    /// <summary>
    /// 无文本文件服务
    /// </summary>
    public class NoTextFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetFileContent(string filePath)
        {
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                // 如果文件存在
                if (fileInfo != null && fileInfo.Exists)
                {
                    System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                    StringBuilder builder = new StringBuilder();
                    builder.Append("文件名称：" + info.FileName.Substring(info.FileName.LastIndexOf("\\") + 1));
                    builder.Append("\r\n更新时间：" + fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    builder.Append("\r\n文件大小：" + Math.Ceiling(fileInfo.Length / 1024.0) + " KB");

                    return builder.ToString();
                }
                return filePath;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return filePath;
            }
        }
    }
}
