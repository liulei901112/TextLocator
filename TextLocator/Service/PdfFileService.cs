using log4net;
using Spire.Pdf;
using System;
using System.IO;
using System.Text;

namespace TextLocator.Service
{
    /// <summary>
    /// PDF文件服务
    /// </summary>
    public class PdfFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetFileContent(string filePath)
        {            
            // 文件内容
            string content = "";
            try
            {
                // 实例化一个PdfDocument对象
                using (PdfDocument doc = new PdfDocument())
                {
                    //实例化一个StringBuilder 对象
                    StringBuilder builder = new StringBuilder();
                    // 加载Pdf文档
                    doc.LoadFromFile(filePath);                    

                    //提取PDF所有页面的文本
                    foreach (PdfPageBase page in doc.Pages)
                    {
                        builder.Append(page.ExtractText());
                    }

                    doc.Dispose();

                    content = builder.ToString();
                }

                
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            return content.Replace("Evaluation Warning : The document was created with Spire.PDF for .NET. ", "");
        }
    }
}
