using log4net;
using Spire.Presentation;
using Spire.Presentation.Collections;
using System;
using System.IO;
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
            StringBuilder builder = new StringBuilder();
            lock (locker)
            {
                try
                {
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        using (Presentation presentation = new Presentation(fs, FileFormat.Auto))
                        {
                            SlideCollection slides = presentation.Slides;
                            if (slides != null && slides.Count > 0)
                            {
                                foreach (ISlide slide in presentation.Slides)
                                {
                                    ShapeCollection shapes = slide.Shapes;
                                    if (shapes != null && shapes.Count > 0)
                                    {
                                        foreach (IShape shape in shapes)
                                        {
                                            if (shape != null && shape is IAutoShape)
                                            {
                                                ITextFrameProperties textFrame;
                                                if ((textFrame = (shape as IAutoShape).TextFrame) != null)
                                                {
                                                    ParagraphCollection paragraph = textFrame.Paragraphs;
                                                    if (paragraph != null && paragraph.Count > 0)
                                                    {
                                                        foreach (TextParagraph tp in paragraph)
                                                        {
                                                            builder.Append(tp.Text + Environment.NewLine);
                                                        }
                                                    }
                                                }
                                            }
                                            shape.Dispose();
                                        }
                                    }
                                    slide.Dispose();
                                }
                            }

                        }
                    }
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
