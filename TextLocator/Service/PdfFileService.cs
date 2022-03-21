using log4net;
using Spire.Pdf;
using Spire.Pdf.Widget;
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
            StringBuilder builder = new StringBuilder();
            lock (locker)
            {
                try
                {
                    
                    // 实例化一个PdfDocument对象
                    using (PdfDocument doc = new PdfDocument(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
                    {
                        PdfPageCollection pages = doc.Pages;
                        if (pages != null && pages.Count > 0)
                        {
                            //提取PDF所有页面的文本
                            foreach (PdfPageBase page in pages)
                            {
                                try
                                {
                                    builder.Append(page.ExtractText().Replace("Evaluation Warning : The document was created with Spire.PDF for .NET.", ""));
                                } catch { }
                            }
                        }
                    }
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
            return builder.ToString();
        }
    }
}
