using log4net;
using Spire.Doc;
using System;
using System.IO;
using System.Text;

namespace TextLocator.Service
{
    /// <summary>
    /// Word文件服务
    /// </summary>
    public class WordFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static object locker = new object();

        public string GetFileContent(string filePath)
        {
            // 文件内容
            string content = "";
            lock (locker)
            {
                try
                {
                    using (var document = new Document(new FileStream(filePath, FileMode.Open)))
                    {
                        // 提取每个段落的文本 
                        StringBuilder builder = new StringBuilder();
                        foreach (Section section in document.Sections)
                        {
                            foreach (Spire.Doc.Documents.Paragraph paragraph in section.Paragraphs)
                            {
                                builder.AppendLine(paragraph.Text);
                            }
                        }
                        content = builder.ToString();

                        document.Close();
                        document.Dispose();                        
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
            return content;
        }
    }
}
