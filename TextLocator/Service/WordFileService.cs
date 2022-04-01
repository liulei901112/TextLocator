using log4net;
using NPOI.XWPF.UserModel;
using Spire.Doc;
using Spire.Doc.Collections;
using Spire.Doc.Documents;
using System;
using System.Collections.Generic;
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
            // 内容
            StringBuilder builder = new StringBuilder();
            lock (locker)
            {
                try
                {
                    // =========== Spire.XLS ===========
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        using (Spire.Doc.Document document = new Spire.Doc.Document(fs))
                        {
                            if (document != null)
                            {
                                SectionCollection sections = document.Sections;
                                if (sections != null && sections.Count > 0)
                                {
                                    // 提取每个段落的文本 
                                    foreach (Section section in sections)
                                    {
                                        ParagraphCollection paragraphs = section.Paragraphs;
                                        if (paragraphs != null && paragraphs.Count > 0)
                                        {
                                            foreach (Paragraph paragraph in paragraphs)
                                            {
                                                builder.AppendLine(paragraph.Text);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception spirex) {
                    log.Error(filePath + " -> " + spirex.Message + " Spire.Xls解析错误，尝试NPIO", spirex);
                    // =========== NPIO ===========
                    XWPFDocument document = null;
                    try
                    {
                        using (FileStream fs = File.OpenRead(filePath))
                        {
                            document = new XWPFDocument(fs);

                            if (document != null)
                            {
                                // 页眉页脚
                                // 表格
                                IList<XWPFTable> tables = document.Tables;
                                if (tables != null && tables.Count > 0)
                                {
                                    foreach(XWPFTable table in tables)
                                    {
                                        List<XWPFTableRow> rows = table.Rows;
                                        if (rows != null && rows.Count > 0)
                                        {
                                            foreach(XWPFTableRow row in rows)
                                            {
                                                List<XWPFTableCell> cells = row.GetTableCells();
                                                if (cells != null && cells.Count > 0) {
                                                    foreach (XWPFTableCell cell in cells)
                                                    {
                                                        builder.Append(cell.GetText() + "　");
                                                    }
                                                }
                                                builder.AppendLine();
                                            }
                                        }
                                        builder.AppendLine();
                                    }
                                }

                                // 正文内容
                                IList<XWPFParagraph> paragraphs = document.Paragraphs;
                                if (paragraphs != null && paragraphs.Count > 0)
                                {
                                    foreach(XWPFParagraph paragraph in paragraphs)
                                    {
                                        builder.AppendLine(paragraph.ParagraphText);
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
            }
            return builder.ToString();
        }
    }
}
