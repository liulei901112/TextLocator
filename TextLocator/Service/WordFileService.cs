using DocumentFormat.OpenXml.Packaging;
using log4net;
using NPOI.XWPF.UserModel;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using TextLocator.Core;
using TextLocator.Factory;
using Word = NetOffice.WordApi;

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
            // 内容
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
                            // 3、=========== NetOffice.WordApi ===========
                            content = NetOfficeParse(filePath); 
                        }
                        catch (Exception ex2)
                        {
                            log.Error(filePath + " -> NetOffice.WordApi 无法解析：" + ex2.Message, ex2);

                            try
                            {
                                // 4、=========== Spire.Doc ===========
                                content = SpireDocParse(filePath);
                            }
                            catch (Exception ex3)
                            {
                                log.Error(filePath + " -> Spire.Doc 无法解析：" + ex3.Message, ex3);
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
            // 创建一个Word应用
            using (Word.Application app = new Word.Application())
            {
                // Type appType = app.GetType();
                try
                {
                    // 参数（缺省）
                    object missing = Missing.Value;
                    // wdGoToItem 枚举 项目类型
                    object objWhat = Word.Enums.WdGoToItem.wdGoToPage;
                    // wdGoToDirection 枚举 位置
                    object objWhich = Word.Enums.WdGoToDirection.wdGoToNext;
                    // 页号
                    object page = null;
                    using (Word.Document doc = app.Documents.Open(filePath, false, true, false, missing, missing, false))
                    {
                        // 获取总页数
                        int totalPage = doc.ComputeStatistics(Word.Enums.WdStatistic.wdStatisticPages);
                        // 按页遍历读取
                        for (int i = 0; i < totalPage; i++)
                        {
                            page = i;

                            // Range 文档中的一个连续区域
                            Word.Range range1 = doc.GoTo(objWhat, objWhich, page);
                            Word.Range range2 = range1.GoToNext(Word.Enums.WdGoToItem.wdGoToPage);

                            // Range.Start 返回或设置范围的其实字符位置
                            object objStart = range1.Start;
                            object objEnd = range2.Start;

                            // 最后一页
                            if (range1.Start == range2.Start)
                            {
                                objEnd = missing;
                            }

                            // Document.Range 使用指定的其实和结束字符位置返回一个对象
                            Word.Range range3 = doc.Range(objStart, objEnd);

                            // Range.Text 返回或设置指定区域的文本
                            builder.Append(range3.Text);

                            // pageInfo
                            builder.AppendLine();
                            builder.AppendLine("----" + (i + 1) + "----");
                            builder.AppendLine();
                        }
                    }
                }
                finally
                {
                    try
                    {
                        // appType.InvokeMember("Quit", System.Reflection.BindingFlags.InvokeMethod, null, app, new object[] { false });
                        app.Quit(false);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Word关闭错误：" + ex.Message, ex);
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
                using (WordprocessingDocument document = WordprocessingDocument.Open(fs, false))
                {
                    NameTable nt = new NameTable();
                    XmlNamespaceManager nsManager = new XmlNamespaceManager(nt);
                    nsManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

                    XmlDocument xmlDoc = new XmlDocument(nt);
                    xmlDoc.Load(document.MainDocumentPart.GetStream());

                    XmlNodeList paragraphNodes = xmlDoc.SelectNodes("//w:p", nsManager);

                    foreach (XmlNode paragraphNode in paragraphNodes)
                    {
                        // XmlNodeList textNodes = paragraphNode.SelectNodes(".//['w: t'&'w:tab']", nsManager);
                        XmlNodeList textNodes = paragraphNode.SelectNodes(".//w:t", nsManager);

                        foreach (XmlNode textNode in textNodes)
                        {
                            builder.AppendLine(textNode.InnerText);
                        }
                        builder.AppendLine();
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// NPOI 解析
        /// </summary>
        /// <filePath name="filePath"></filePath>
        /// <returns></returns>
        private string NPOIParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
                XWPFDocument docx = new XWPFDocument(fs);

                /*// 页眉
                foreach (XWPFHeader xwpfHeader in docx.HeaderList)
                {
                    builder.AppendLine(string.Format("{0}", new string[] { xwpfHeader.Text }));
                }

                // 页脚

                foreach (XWPFFooter xwpfFooter in docx.FooterList)
                {
                    builder.AppendLine(string.Format("{0}", new string[] { xwpfFooter.Text }));
                }
                */
                // 读取段落
                foreach (var para in docx.Paragraphs)
                {
                    // 获得文本
                    string text = para.ParagraphText;
                    var runs = para.Runs;
                    // string styleid = para.Style;
                    for (int i = 0; i < runs.Count; i++)
                    {
                        var run = runs[i];
                        // 获得run的文本
                        text = run.ToString();
                        builder.Append(text + ",");
                    }
                    builder.AppendLine();
                }
                // 读取表格
                foreach (XWPFTable table in docx.Tables)
                {
                    // 循环表格行
                    foreach (XWPFTableRow row in table.Rows)
                    {
                        foreach (XWPFTableCell cell in row.GetTableCells())
                        {
                            builder.Append(cell.GetText());
                        }
                    }
                }

                // 读取图片
                foreach (XWPFPictureData pictureData in docx.AllPictures)
                {
                    string picExtName = pictureData.SuggestFileExtension();
                    string picFileName = pictureData.FileName;
                    string picTempName = string.Format(Guid.NewGuid().ToString() + "_" + picFileName + "." + picExtName);

                    /*byte[] picFileContent = pictureData.Data;
                    using (FileStream fs = new FileStream(picTempName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(picFileContent, 0, picFileContent.Length);
                        fs.Close();
                    }*/

                    builder.AppendLine(picTempName);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Spire.Doc 解析
        /// </summary>
        /// <filePath name="filePath"></filePath>
        /// <returns></returns>
        private string SpireDocParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (Spire.Doc.Document document = new Spire.Doc.Document(fs))
                {
                    if (document != null)
                    {
                        Spire.Doc.Collections.SectionCollection sections = document.Sections;
                        if (sections != null && sections.Count > 0)
                        {
                            // 提取每个段落的文本 
                            int page = 1;
                            foreach (Spire.Doc.Section section in sections)
                            {
                                Spire.Doc.Collections.ParagraphCollection paragraphs = section.Paragraphs;
                                if (paragraphs != null && paragraphs.Count > 0)
                                {
                                    foreach (Spire.Doc.Documents.Paragraph paragraph in paragraphs)
                                    {
                                        builder.AppendLine(paragraph.Text);
                                    }
                                }
                                builder.AppendLine();
                                builder.AppendLine("----" + page + "----");
                                builder.AppendLine();
                                page++;
                            }
                        }
                    }
                }
            }
            return builder.ToString();
        }
    }
}
