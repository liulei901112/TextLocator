using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextLocator.Consts;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.Util;

namespace TextLocator.Index
{
    /// <summary>
    /// Lucence 索引核心工具类
    /// </summary>
    public class LuceneIndexCore
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 状态回调委托
        /// </summary>
        /// <param name="status"></param>
        public delegate void StatusCallback(string status);

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="files">获得的文档包</param>
        public static void CreateIndex(List<FileInfo> files, bool rebuild, StatusCallback callback = null)
        {
            // 判断是创建索引还是增量索引（如果索引目录不存在，重建）
            bool create = !Directory.Exists(AppConst.APP_INDEX_DIR);
            // 入参为true，表示重建
            if (rebuild)
            {
                create = rebuild;
            }

            // 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
            Lucene.Net.Index.IndexWriter writer = new Lucene.Net.Index.IndexWriter(AppConst.INDEX_DIRECTORY, AppConst.INDEX_ANALYZER, create, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

            // 遍历读取文件，并创建索引
            for (int i = 0; i < files.Count(); i++)
            {
                FileInfo fileInfo = files[i];

                // 文件名
                string fileName = fileInfo.Name;
                // 文件路径
                string filePath = fileInfo.DirectoryName + "\\" + fileName;
                // 文件大小
                long fileSize = fileInfo.Length;
                // 创建时间
                string createTime = fileInfo.CreationTime.ToString("yyyy-MM-dd");
                // 文件标记
                string fileMark = fileInfo.DirectoryName + fileInfo.CreationTime.ToString();

                // 执行状态回调
                callback("索引：" + filePath);

                // 根据文件路径获取文件类型（自定义文件类型分类）
                FileType fileType = FileTypeUtil.GetFileType(filePath);

                // 文件内容
                string content = FileInfoServiceFactory.GetFileInfoService(fileType).GetFileContent(filePath);

                // 缩略信息
                string breviary = content.Replace(" ", "");
                if (breviary.Length > 335)
                {
                    breviary = breviary.Substring(0, 335);
                }

                Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();


                // 当索引文件中含有与filemark相等的field值时，会先删除再添加，以防出现重复
                writer.DeleteDocuments(new Lucene.Net.Index.Term("FileMark", fileMark));
                // 不分词建索引
                doc.Add(new Lucene.Net.Documents.Field("FileMark", fileMark, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("FileType", fileType.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("FileSize", fileSize + "", Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("Breviary", breviary, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("CreateTime", createTime, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                // ANALYZED分词建索引
                doc.Add(new Lucene.Net.Documents.Field("FileName", fileName, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("FilePath", filePath, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("Content", content, Lucene.Net.Documents.Field.Store.NO, Lucene.Net.Documents.Field.Index.ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("Breviary", breviary, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                
                writer.AddDocument(doc);
                // 优化索引
                writer.Optimize();
            }
            writer.Dispose();
        }
    }
}
