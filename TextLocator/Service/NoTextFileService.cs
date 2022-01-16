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
                    builder.Append(" 文件名称：" + info.FileName);
                    builder.Append(" 产品名称：" + info.ProductName);
                    builder.Append(" 公司名称：" + info.CompanyName);
                    builder.Append(" 文件版本：" + info.FileVersion);
                    builder.Append(" 产品版本：" + info.ProductVersion);
                    // 通常版本号显示为「主版本号.次版本号.生成号.专用部件号」
                    builder.Append(" 系统显示文件版本：" + info.ProductMajorPart + '.' + info.ProductMinorPart + '.' + info.ProductBuildPart + '.' + info.ProductPrivatePart);
                    builder.Append(" 文件说明：" + info.FileDescription);
                    builder.Append(" 文件语言：" + info.Language);
                    builder.Append(" 原始文件名称：" + info.OriginalFilename);
                    builder.Append(" 文件版权：" + info.LegalCopyright);
                    builder.Append(" 文件大小：" + Math.Ceiling(fileInfo.Length / 1024.0) + " KB");

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
