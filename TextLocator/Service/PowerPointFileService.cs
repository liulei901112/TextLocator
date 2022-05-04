using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using TextLocator.Core;
using PPT = NetOffice.PowerPointApi;

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
            string content = string.Empty;
            lock (locker)
            {
                try
                {
                    // 1、=========== DocumentFormat.OpenXml ===========
                    content = OpenXmlParse(filePath);
                }
                catch (Exception ex)
                {
                    log.Error(filePath + " -> DocumentFormat.OpenXml 无法解析：" + ex.Message, ex);
                    try
                    {
                        // 2、=========== NPOI ===========                        
                        content = NPOIParse(filePath);
                    }
                    catch (Exception ex1)
                    {
                        log.Error(filePath + " -> NPOI 无法解析：" + ex1.Message, ex1);
                        try
                        {
                            // 3、=========== NetOffice.PowerPointApi ===========
                            content = NetOfficeParse(filePath);
                        }
                        catch (Exception ex2)
                        {
                            log.Error(filePath + " -> NetOffice.PowerPointApi 无法解析：" + ex2.Message, ex2);
                            try
                            {
                                // 4、=========== Spire.Presentation ===========
                                content = SpirePresentParse(filePath);
                            }
                            catch (Exception ex3)
                            {
                                log.Error(filePath + " -> Spire.Presentation 无法解析：" + ex3.Message, ex3);
                            }
                        }
                    }
                }
            }
            return content;
        }

        /// <summary>
        /// NetOffice 解析
        /// </summary>
        /// <filePath name="filePath"></filePath>
        /// <returns></returns>
        private string NetOfficeParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (PPT.Application app = new PPT.Application())
            {
                // Type appType = app.GetType();
                try
                {
                    // 参数（缺省）
                    object missing = Missing.Value;

                    using (PPT.Presentation ppt = app.Presentations.Open(filePath, true, missing, false))
                    {
                        int page = 1;
                        // 遍历页
                        foreach (PPT.Slide slide in ppt.Slides)
                        {
                            // 遍历元素
                            foreach (PPT.Shape shape in slide.Shapes)
                            {
                                // 判断是否是文字
                                if (shape.HasTextFrame == NetOffice.OfficeApi.Enums.MsoTriState.msoTrue && shape.TextFrame.HasText == NetOffice.OfficeApi.Enums.MsoTriState.msoTrue)
                                {
                                    builder.Append(shape.TextFrame.TextRange.Text);
                                }
                                // 判断是否是表格
                                if (shape.HasTable == NetOffice.OfficeApi.Enums.MsoTriState.msoTrue)
                                {
                                    builder.AppendLine();
                                    // 遍历行
                                    for(int i = 0; i < shape.Table.Rows.Count; i++)
                                    {
                                        // 遍历列
                                        for (int j = 0; j < shape.Table.Columns.Count; j++)
                                        {
                                            builder.Append(shape.Table.Cell(i + 1, j + 1).Shape.TextFrame.TextRange.Text + " ");
                                        }
                                        builder.AppendLine();
                                    }                                    
                                    builder.AppendLine();
                                }
                            }
                            builder.AppendLine();
                            builder.AppendLine("----" + page + "----");
                            builder.AppendLine();
                            page++;
                        }
                    }
                }
                finally
                {
                    try
                    {
                        // appType.InvokeMember("Quit", System.Reflection.BindingFlags.InvokeMethod, null, app, new object[] { false });
                        app.Quit();
                    }
                    catch (Exception ex)
                    {
                        log.Error("PPT关闭错误：" + ex.Message, ex);
                    }

                    AppCore.ManualGC();
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// DocumentFormat.OpenXml 解析
        /// </summary>
        /// <filePath name="filePath">FileInfoServicefilePath</filePath>
        /// <returns></returns>
        private string OpenXmlParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (var presentationDocument = PresentationDocument.Open(fs, false))
                {
                    var presentationPart = presentationDocument.PresentationPart;
                    var presentation = presentationPart.Presentation;

                    // 遍历页面
                    int page = 1;
                    foreach (var slideId in presentation.SlideIdList.ChildElements.OfType<SlideId>())
                    {
                        // 获取页面内容
                        SlidePart slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId);

                        var slide = slidePart.Slide;

                        foreach (var paragraph in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                        {
                            // 获取段落
                            // 在 PPT 文本是放在形状里面
                            foreach (var text in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                            {
                                // 获取段落文本，这样不会添加文本格式
                                builder.Append(text.Text);
                            }
                        }
                        builder.AppendLine();
                        builder.AppendLine("----" + page + "----");
                        builder.AppendLine();
                        page++;
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// NPOIParse 解析
        /// </summary>
        /// <filePath name="filePath">FileInfoServicefilePath</filePath>
        /// <returns></returns>
        private string NPOIParse(string filePath)
        {
            /*StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
            }
            return builder.ToString();*/
            throw new NotImplementedException("NPOI 暂无PPT解析实现");
        }

        /// <summary>
        /// Spire.XLS 解析
        /// </summary>
        /// <filePath name="filePath"></filePath>
        /// <returns></returns>
        private string SpirePresentParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (Spire.Presentation.Presentation presentation = new Spire.Presentation.Presentation(fs, Spire.Presentation.FileFormat.Auto))
                {
                    Spire.Presentation.Collections.SlideCollection slides = presentation.Slides;
                    if (slides != null && slides.Count > 0)
                    {
                        int page = 1;
                        foreach (Spire.Presentation.ISlide slide in presentation.Slides)
                        {
                            Spire.Presentation.ShapeCollection shapes = slide.Shapes;
                            if (shapes != null && shapes.Count > 0)
                            {
                                foreach (Spire.Presentation.IShape shape in shapes)
                                {
                                    if (shape != null && shape is Spire.Presentation.IAutoShape)
                                    {
                                        Spire.Presentation.ITextFrameProperties textFrame;
                                        if ((textFrame = (shape as Spire.Presentation.IAutoShape).TextFrame) != null)
                                        {
                                            Spire.Presentation.Collections.ParagraphCollection paragraph = textFrame.Paragraphs;
                                            if (paragraph != null && paragraph.Count > 0)
                                            {
                                                foreach (Spire.Presentation.TextParagraph tp in paragraph)
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
                            builder.AppendLine();
                            builder.AppendLine("----" + page + "----");
                            builder.AppendLine();
                            page++;
                        }
                    }

                }
            }
            return builder.ToString();
        }
    }
}
