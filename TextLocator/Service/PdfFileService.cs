using java.io;
using log4net;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using System;
using System.IO;
using System.Text;
using TextLocator.Factory;

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
            string content = string.Empty;
            lock (locker)
            {
                try
                {
                    // =========== PdfBOX ===========尝试解析
                    content = PdfBOXParse(filePath);
                }
                catch (Exception ex1)
                {
                    log.Error(filePath + " -> PdfBOX 无法解析：" + ex1.Message, ex1);
                    
                    try
                    {
                        // =========== Spire ===========
                        content = SpirePdfParse(filePath);
                    }
                    catch (Exception ex)
                    {
                        log.Error(filePath + " -> 无法解析：" + ex.Message, ex);
                    }
                }
            }
            return content;
        }

        /// <summary>
        /// PdfBOX 解析
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string PdfBOXParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (PDDocument pdf = PDDocument.load(filePath))
            {
                PDFTextStripper pdfText = new PDFTextStripper();
                builder.Append(pdfText.getText(pdf));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Spire.PDF 解析
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string SpirePdfParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = System.IO.File.OpenRead(filePath))
            {
                // 实例化一个PdfDocument对象
                using (Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument(fs))
                {
                    Spire.Pdf.Widget.PdfPageCollection pages = doc.Pages;
                    if (pages != null && pages.Count > 0)
                    {
                        //提取PDF所有页面的文本
                        foreach (Spire.Pdf.PdfPageBase page in pages)
                        {
                            try
                            {
                                builder.Append(page.ExtractText().Replace("Evaluation Warning : The document was created with Spire.PDF for .NET.", ""));
                            }
                            catch { }
                        }
                    }
                }
            }
            return builder.ToString();
        }
    }
}
