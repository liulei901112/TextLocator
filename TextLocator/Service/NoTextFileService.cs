using log4net;
using System;
using System.Text;
using TextLocator.Factory;
using TextLocator.Util;

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
                string fileName = fileInfo.Name;
                StringBuilder builder = new StringBuilder();
                builder.Append("文件名称：" + fileName);
                builder.Append("；更新时间：" + fileInfo.LastWriteTime.ToString("yyyy-MM-dd　HH:mm:ss"));
                builder.Append("；文件大小：" + FileUtil.GetFileSizeFriendly(fileInfo.Length));

                return builder.ToString();
            }
            catch (Exception ex)
            {
                log.Error(filePath + " -> " + ex.Message, ex);                
            }
            return filePath;
        }
    }
}
