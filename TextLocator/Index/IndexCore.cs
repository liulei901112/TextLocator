using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TextLocator.Core;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.Index;
using TextLocator.Jieba;
using TextLocator.Service;
using TextLocator.Util;

namespace TextLocator.Index
{
    /// <summary>
    /// Lucence 索引核心工具类
    /// </summary>
    public class IndexCore
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 状态回调委托
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="percent"></param>
        public delegate void Callback(string msg, double percent);

        /// <summary>
        /// 锁
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// 已完成数量
        /// </summary>
        private static volatile int finishCount = 0;

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="filePaths">获得的文档包</param>
        public static void CreateIndex(List<string> filePaths, bool rebuild, Callback callback)
        {
            // 判断是创建索引还是增量索引（如果索引目录不存在，重建）
            bool create = !Directory.Exists(AppConst.APP_INDEX_DIR);
            // 入参为true，表示重建
            if (rebuild)
            {
                create = rebuild;
            }

            // 重建或创建
            if (create)
            {
                // 重建时，删除全部已建索引的标记
                AppUtil.DeleteSection("FileIndex");
            }

            // 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
            Lucene.Net.Index.IndexWriter indexWriter = new Lucene.Net.Index.IndexWriter(
                AppConst.INDEX_DIRECTORY, 
                AppConst.INDEX_ANALYZER, 
                create, 
                Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

            indexWriter.SetRAMBufferSizeMB(512);
            indexWriter.SetMaxBufferedDocs(1024);

            // 文件总数
            int totalCount = filePaths.Count();

            // 每次初始化的时候完成数量都是0
            finishCount = 0;

            using (MutipleThreadResetEvent resetEvent = new MutipleThreadResetEvent(totalCount))
            {
                // 遍历读取文件，并创建索引
                for (int i = 0; i < totalCount; i++)
                {
                    string filePath = filePaths[i];
                    // 忽略已存在索引的文件
                    if (SkipFile(create, filePath, totalCount, callback, resetEvent))
                    {
                        continue;
                    }
                    // 加入线程池
                    ThreadPool.QueueUserWorkItem(new WaitCallback(CreateIndexTask), new TaskInfo() {
                        TotalCount = totalCount,
                        FilePath = filePaths[i],
                        IndexWriter = indexWriter,
                        Callback = callback,
                        ResetEvent = resetEvent
                    });
                }

                // 等待所有线程结束
                resetEvent.WaitAll();

                // 销毁
                resetEvent.Dispose();
            }

            try
            {
                // 索引优化
                indexWriter.Optimize();
                // 索引写入器销毁
                indexWriter.Dispose();

                // 手动GC
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// 忽略文件
        /// </summary>
        /// <param name="create">是否是创建，true为创建、false为更新</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="totalCount">文件总数</param>
        /// <param name="callback">状态回调函数</param>
        /// <param name="resetEvent">多线程任务标记</param>
        private static bool SkipFile(bool create, string filePath, int totalCount, Callback callback, MutipleThreadResetEvent resetEvent)
        {
            // 非重建 && 文件已经被索引过
            bool isUpdate = !create;
            bool isExists = !string.IsNullOrEmpty(AppUtil.ReadValue("FileIndex", filePath, ""));
            if (isUpdate && isExists)
            {
                string skipMsg = "跳过文件：" + filePath;

                callback(skipMsg, CalcCompletionRatio(finishCount, totalCount));

                lock (locker)
                {
                    finishCount++;
                }

                try
                {
                    resetEvent.SetOne();
                }
                catch { }

#if !DEBUG
                log.Debug(skipMsg);
#endif
                return true;
            }
            return false;
        }

        /// <summary>
        /// 创建索引任务方法
        /// </summary>
        /// <param name="obj"></param>
        private static void CreateIndexTask(object obj)
        {
            TaskInfo taskInfo = obj as TaskInfo;
            try
            {
                // 开始时间1
                var taskMark = TaskTime.StartNew();

                // 索引写入
                Lucene.Net.Index.IndexWriter indexWriter = taskInfo.IndexWriter;

                // 文件路径
                string filePath = taskInfo.FilePath;

                // 写入
                AppUtil.WriteValue("FileIndex", filePath, "1");

                // 文件信息
                FileInfo fileInfo = new FileInfo(filePath);
                // 文件名
                string fileName = fileInfo.Name;
                // 文件大小
                long fileSize = fileInfo.Length;
                // 创建时间
                string createTime = fileInfo.CreationTime.ToString("yyyy-MM-dd");                

                // 根据文件路径获取文件类型（自定义文件类型分类）
                FileType fileType = FileTypeUtil.GetFileType(filePath);

                string filePathPadding = filePath;
                try
                {
                    filePathPadding = filePath.Substring(0, 35) + "......" + filePath.Substring(filePath.Length - 35);
                }
                catch { }

                StringBuilder msg = new StringBuilder("[" + finishCount * 1.0F + "/" + taskInfo.TotalCount + "] => 引擎：" + (int)fileType + "，文件：" + filePathPadding);

                // 文件内容
                string content = FileInfoServiceFactory.GetFileInfoService(fileType).GetFileContent(filePath);

                msg.Append("，解析：" + taskMark.ConsumeTime + "秒");

                // 缩略信息
                string breviary = AppConst.REGIX_LINE_BREAKS_AND_WHITESPACE.Replace(content, "");
                if (breviary.Length > 120)
                {
                    breviary = breviary.Substring(0, 120) + "...";
                }

                // 文件标记
                string fileMark = MD5Util.GetMD5Hash(filePath); //fileInfo.DirectoryName + fileInfo.CreationTime.ToString();

                // 开始时间2
                taskMark = TaskTime.StartNew();

                lock (locker)
                {
                    // 当索引文件中含有与filemark相等的field值时，会先删除再添加，以防出现重复
                    indexWriter.DeleteDocuments(new Lucene.Net.Index.Term("FileMark", fileMark));

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

                    indexWriter.AddDocument(doc);
                }
                msg.Append("，索引：" + taskMark.ConsumeTime + "秒");

                // 执行状态回调
                taskInfo.Callback(msg.ToString(), CalcCompletionRatio(finishCount, taskInfo.TotalCount)); ;

                log.Debug(msg);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            finally
            {
                lock (locker)
                {
                    finishCount++;
                }

                try
                {
                    taskInfo.ResetEvent.SetOne();
                }
                catch { }              
            }
        }

        /// <summary>
        /// 计算完成比例
        /// </summary>
        /// <param name="finishCount"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        private static double CalcCompletionRatio(double finishCount, double totalCount)
        {
            return finishCount * 1.00F / totalCount * 1.00F * 100.00F;
        }

        /// <summary>
        /// 任务信息
        /// </summary>
        class TaskInfo
        {
            /// <summary>
            /// 文件总数
            /// </summary>
            public int TotalCount { get; set; }
            /// <summary>
            /// 文件路径
            /// </summary>
            public string FilePath { get; set; }
            /// <summary>
            /// 索引写入器
            /// </summary>
            public Lucene.Net.Index.IndexWriter IndexWriter { get; set; }
            /// <summary>
            /// 回调函数
            /// </summary>
            public Callback Callback { get; set; }
            /// <summary>
            /// 多线程重置
            /// </summary>
            public MutipleThreadResetEvent ResetEvent { get; set; }
        }
    }
}
