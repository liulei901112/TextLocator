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

        private static volatile object locker = new object();

        public string GetFileContent(string filePath)
        {            
            // 文件内容
            string content = "";
            lock (locker)
            {
                try
                {
                    //实例化一个StringBuilder 对象
                    StringBuilder builder = new StringBuilder();
                    // 实例化一个PdfDocument对象
                    using (PdfDocument doc = new PdfDocument())
                    {                        
                        // 加载Pdf文档
                        doc.LoadFromFile(filePath);

                        //提取PDF所有页面的文本
                        foreach (PdfPageBase page in doc.Pages)
                        {
                            builder.Append(page.ExtractText().Replace("Evaluation Warning : The document was created with Spire.PDF for .NET.", ""));
                        }
                    }
                    content = builder.ToString();
                }
                catch (ObjectDisposedException ex)
                {
                    log.Error(filePath + " -> " + ex.Message, ex);
                }
                catch (Exception ex)
                {
                    log.Error(filePath + " -> " + ex.Message, ex);
                }
            }
            return content;
        }
    }
}
