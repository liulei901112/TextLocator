using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TextLocator.Core;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.Service;
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
        /// <param name="filePaths">获得的文档包</param>
        public static void CreateIndex(List<string> filePaths, bool rebuild, StatusCallback callback)
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

            // 文件总数
            int count = filePaths.Count();

            // 遍历读取文件，并创建索引
            for (int i = 0; i < count; i++)
            {
                // 文件路径
                string filePath = filePaths[i];

                // 临时文件跳过
                if (filePath.IndexOf("~$") >0)
                {
                    continue;
                }
                // 非重建 && 文件已经被索引过
                if (!create && !string.IsNullOrEmpty(AppUtil.ReadIni("FileIndex", filePath, "")))
                {
                    continue;
                }

                // 写入
                AppUtil.WriteIni("FileIndex", filePath, "1");

                // 开始时间
                DateTime beginMark = DateTime.Now;

                // 文件信息
                FileInfo fileInfo = new FileInfo(filePath);

                // 文件名
                string fileName = fileInfo.Name;
                // 文件大小
                long fileSize = fileInfo.Length;
                // 创建时间
                string createTime = fileInfo.CreationTime.ToString("yyyy-MM-dd");
                // 文件标记
                string fileMark = fileInfo.DirectoryName + fileInfo.CreationTime.ToString();

                // 根据文件路径获取文件类型（自定义文件类型分类）
                FileType fileType = FileTypeUtil.GetFileType(filePath);

                IFileInfoService fileInfoService = FileInfoServiceFactory.GetFileInfoService(fileType);

                // 文件内容
                string content = fileInfoService.GetFileContent(filePath);

                // 缩略信息
                string breviary = new Regex(" |\r|\n|\\s").Replace(content, "");
                if (breviary.Length > 150)
                {
                    breviary = breviary.Substring(0, 150) + "...";
                }

                // 当索引文件中含有与filemark相等的field值时，会先删除再添加，以防出现重复
                writer.DeleteDocuments(new Lucene.Net.Index.Term("FileMark", fileMark));

                Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
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

                writer.AddDocument(doc);
                // 优化索引
                writer.Optimize();

                string msg = "索引：[" + i + " / " + count + "] => 文件：" + filePath + "，耗时：" + (DateTime.Now - beginMark).TotalSeconds + "秒";

                // 执行状态回调
                callback(msg);

                log.Debug(msg);                

                // 手动GC
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            try
            {
                writer.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }
    }
}
