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

            // 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
            Lucene.Net.Index.IndexWriter indexWriter = new Lucene.Net.Index.IndexWriter(
                AppConst.INDEX_DIRECTORY, 
                AppConst.INDEX_ANALYZER, 
                create, 
                Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

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
                    // 非重建 && 文件已经被索引过
                    bool isUpdate = !create;
                    bool isExists = !string.IsNullOrEmpty(AppUtil.ReadIni("FileIndex", filePath, ""));
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
                        continue;
                    }
                    // 加入线程池
                    ThreadPool.QueueUserWorkItem(new WaitCallback(CreateIndexTask), new TaskInfo() {
                        Create = create,
                        TotalCount = totalCount,
                        FilePath = filePaths[i],
                        IndexWriter = indexWriter,
                        Callback = callback,
                        ResetEvent = resetEvent
                    });
                    /*new Thread(()=> {
                        CreateIndexTask(new TaskInfo()
                        {
                            Create = create,
                            TotalCount = totalCount,
                            FilePath = filePaths[i],
                            IndexWriter = indexWriter,
                            Callback = callback,
                            ResetEvent = resetEvent
                        });
                    }).Start();*/
                }

                // 等待所有线程结束
                resetEvent.WaitAll();

                // 销毁
                resetEvent.Dispose();
            }

            try
            {
                indexWriter.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
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
                Lucene.Net.Index.IndexWriter indexWriter = taskInfo.IndexWriter;

                string filePath = taskInfo.FilePath;

                // 写入
                AppUtil.WriteIni("FileIndex", filePath, "1");

                // 开始时间
                var taskMark = TaskTime.StartNew();

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

                // 文件内容
                string content = FileInfoServiceFactory.GetFileInfoService(fileType).GetFileContent(filePath);

                // 缩略信息
                string breviary = AppConst.REGIX_LINE_BREAKS_AND_WHITESPACE.Replace(content, "");
                if (breviary.Length > 150)
                {
                    breviary = breviary.Substring(0, 150) + "...";
                }

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
                    // 优化索引
                    indexWriter.Optimize();
                }

                string msg = "索引：[" + finishCount * 1.0F + "/" + taskInfo.TotalCount + "] => 引擎：" + (int)fileType + "，文件：" + filePath + "，耗时：" + taskMark.ConsumeTime + "秒";

                // 执行状态回调
                taskInfo.Callback(msg, CalcCompletionRatio(finishCount, taskInfo.TotalCount));

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

                // 手动GC
                GC.Collect();
                GC.WaitForPendingFinalizers();                
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
            public bool Create { get; set; }
            public int TotalCount { get; set; }
            public string FilePath { get; set; }
            public Lucene.Net.Index.IndexWriter IndexWriter { get; set; }
            public Callback Callback { get; set; }
            public MutipleThreadResetEvent ResetEvent { get; set; }
        }
    }
}
