using log4net;
using Lucene.Net.Index;
using Spire.Doc;
using System;
using System.IO;
using System.Text;

namespace TextLocator.Service
{
    /// <summary>
    /// Word服务
    /// </summary>
    public class WordService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetFileContent(string filePath)
        {
            // 文件内容
            string content = "";
            try
            {
                using (var document = new Document(new FileStream(filePath, FileMode.Open)))
                {
                    // 提取每个段落的文本 
                    var sb = new StringBuilder();
                    foreach (Section section in document.Sections)
                    {
                        foreach (Spire.Doc.Documents.Paragraph paragraph in section.Paragraphs)
                        {
                            sb.AppendLine(paragraph.Text);
                        }
                    }
                    content = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            log.Debug(filePath + " => " + content);
            return content;
        }

        public Lucene.Net.Documents.Document GetIndexDocument(FileInfo fileInfo)
        {
            // 文件名
            string fileName = fileInfo.Name;
            string filePath = fileInfo.DirectoryName + "\\" + fileName;
            long fileSize = fileInfo.Length;
            string createTime = fileInfo.CreationTime.ToString("yyyy-MM-dd");

            // 文件内容
            string content = GetFileContent(filePath);

            // 缩略信息
            string breviary = content.Length > 335 ? content.Substring(0, 335) : content;

            Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();

            // 不分词建索引
            doc.Add(new Lucene.Net.Documents.Field("FileSize", fileSize + "", Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
            doc.Add(new Lucene.Net.Documents.Field("Breviary", breviary, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
            doc.Add(new Lucene.Net.Documents.Field("CreateTime", createTime, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

            // ANALYZED分词建索引
            doc.Add(new Lucene.Net.Documents.Field("FileName", fileName, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
            doc.Add(new Lucene.Net.Documents.Field("FilePath", filePath, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
            doc.Add(new Lucene.Net.Documents.Field("Content", content, Lucene.Net.Documents.Field.Store.NO, Lucene.Net.Documents.Field.Index.ANALYZED));
            doc.Add(new Lucene.Net.Documents.Field("Breviary", breviary, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));

            return doc;
        }
    }
}
