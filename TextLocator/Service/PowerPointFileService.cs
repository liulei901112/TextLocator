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

        public string GetFileContent(string filePath)
        {
            // 文件内容
            string content = "";
            try
            {
                Presentation presentation = new Presentation(filePath, FileFormat.Auto);
                StringBuilder builder = new StringBuilder();
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
                    }
                }

                content = builder.ToString();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            return content;
        }
    }
}
