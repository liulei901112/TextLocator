using log4net;
using Spire.Presentation;
using System;
using System.Text;

namespace TextLocator.Service
{
    /// <summary>
    /// PowerPoint文件服务
    /// </summary>
    public class PowerPointFileService : IFileInfoService
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
                    StringBuilder builder = new StringBuilder();
                    using (Presentation presentation = new Presentation(filePath, FileFormat.Auto))
                    {
                        
                        foreach (ISlide slide in presentation.Slides)
                        {
                            foreach (IShape shape in slide.Shapes)
                            {
                                if (shape is IAutoShape)
                                {
                                    if ((shape as IAutoShape).TextFrame != null)
                                    {
                                        foreach (TextParagraph tp in (shape as IAutoShape).TextFrame.Paragraphs)
                                        {
                                            builder.Append(tp.Text + Environment.NewLine);
                                        }
                                    }
                                }
                                shape.Dispose();
                            }
                            slide.Dispose();
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
