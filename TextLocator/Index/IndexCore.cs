using log4net;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TextLocator.Core;
using TextLocator.Enums;
using TextLocator.Factory;
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
        /// 锁对象
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// 完成数量
        /// </summary>
        private static volatile int _finishCount = 0;
        /// <summary>
        /// 文件总数
        /// </summary>
        private static volatile int _totalCount = 0;
        /// <summary>
        /// 是否是创建
        /// </summary>
        private static volatile bool _create = false;
        /// <summary>
        /// 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
        /// </summary>
        private static IndexWriter _indexWriter;
        /// <summary>
        /// 回调函数
        /// </summary>
        private static Callback _callback;

        #region 索引写入器
        /// <summary>
        /// 创建索引写入器
        /// </summary>
        private static void CreateIndexWriter()
        {
            if (_indexWriter == null)
            {
                // 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
                _indexWriter = new IndexWriter(
                    AppConst.INDEX_DIRECTORY,
                    AppConst.INDEX_ANALYZER,
                    _create,
                    IndexWriter.MaxFieldLength.UNLIMITED);

                // 设置Buffer内存上限,默认值16MB
                _indexWriter.SetRAMBufferSizeMB(512);
                // 设置使用复合文件为禁用，
                _indexWriter.UseCompoundFile = false;
            }
        }

        /// <summary>
        /// 关闭索引写入器
        /// </summary>
        private static void CloseIndexWriter()
        {
            if (_indexWriter != null)
            {
                // 关闭索引写入器
                _indexWriter.Close();
                // 销毁索引写入器
                _indexWriter.Dispose();
                // 置为NULL
                _indexWriter = null;
            }
        }
        #endregion

        #region 创建文件索引
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="filePaths">文件列表</param>
        /// <param name="rebuild">是否重建</param>
        /// <param name="callback">消息回调</param>
        public static void CreateIndex(List<string> filePaths, bool rebuild, Callback callback)
        {
            // 记录全局回调函数
            _callback = callback;

            // 文件总数
            _totalCount = filePaths.Count();

            // 每次初始化的时候完成数量都是0
            _finishCount = 0;

            // 判断是创建索引还是增量索引（如果索引目录不存在，重建）
            _create = !Directory.Exists(AppConst.APP_INDEX_DIR);
            // 入参为true，表示重建
            if (rebuild)
            {
                _create = rebuild;
            }

            // 创建还是更新？
            if (_create)
            {
                // 重建时，删除全部已建索引的标记
                AppUtil.DeleteSection("FileIndex");
            }

            // 创建索引写入器
            CreateIndexWriter();

            using (MutipleThreadResetEvent resetEvent = new MutipleThreadResetEvent(_totalCount))
            {
                // 遍历读取文件，并创建索引
                for (int i = 0; i < _totalCount; i++)
                {
                    string filePath = filePaths[i];
                    // 忽略已存在索引的文件
                    if (SkipFile(_create, filePath, resetEvent))
                    {
                        continue;
                    }
                    // 加入线程池
                    ThreadPool.QueueUserWorkItem(new WaitCallback(CreateIndexTask), new TaskInfo() {
                        FilePath = filePath,
                        ResetEvent = resetEvent
                    });
                }

                // 等待所有线程结束
                resetEvent.WaitAll();
            }

            try
            {
                // 索引优化
                _indexWriter.Optimize(10000);

                // 关闭并销毁索引写入器
                CloseIndexWriter();

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
        /// <param name="resetEvent">多线程任务标记</param>
        private static bool SkipFile(bool create, string filePath, MutipleThreadResetEvent resetEvent)
        {
            // 非重建 && 文件已经被索引过
            bool isUpdate = !create;
            bool isExists = !string.IsNullOrEmpty(AppUtil.ReadValue("FileIndex", filePath, ""));
            if (isUpdate && isExists)
            {
                string skipMsg = "跳过文件：" + filePath;

                // 跳过的文件闪烁
                _callback(skipMsg, CalcFinishRatio(_finishCount, _totalCount));

                lock (locker)
                {
                    _finishCount++;
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

            // 文件路径
            string filePath = taskInfo.FilePath;
            try
            {
                // 解析时间
                var taskMark = TaskTime.StartNew();

                // 写入已索引标记
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
                    filePathPadding = filePath.Substring(0, 30) + "......" + filePath.Substring(filePath.Length - 30);
                }
                catch { }

                StringBuilder msg = new StringBuilder("[" + _finishCount * 1.0F + "/" + _totalCount + "] => 引擎：" + (int)fileType + "，文件：" + filePathPadding);

                // 文件内容
                string content = FileInfoServiceFactory.GetFileInfoService(fileType).GetFileContent(filePath);

                msg.Append("，解析：" + taskMark.ConsumeTime + "秒");

                // 缩略信息
                string breviary = AppConst.REGEX_LINE_BREAKS_AND_WHITESPACE.Replace(content, "");
                if (breviary.Length > AppConst.FILE_CONTENT_SUB_LENGTH)
                {
                    breviary = breviary.Substring(0, AppConst.FILE_CONTENT_SUB_LENGTH) + "...";
                }

                // 文件标记
                string fileMark = MD5Util.GetMD5Hash(filePath); //fileInfo.DirectoryName + fileInfo.CreationTime.ToString();

                // 索引时间
                taskMark = TaskTime.StartNew();

                lock (locker)
                {
                    // 当索引文件中含有与filemark相等的field值时，会先删除再添加，以防出现重复
                    // _indexWriter.DeleteDocuments(new Term("FileMark", fileMark));

                    Document doc = new Document();
                    // 不分词建索引
                    doc.Add(new Field("FileMark", fileMark, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.YES));
                    doc.Add(new Field("FileType", fileType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field("FileSize", fileSize + "", Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field("Breviary", breviary, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field("CreateTime", createTime, Field.Store.YES, Field.Index.NOT_ANALYZED));

                    // ANALYZED分词建索引
                    doc.Add(new Field("FileName", fileName, Field.Store.YES, Field.Index.ANALYZED));
                    doc.Add(new Field("FilePath", filePath, Field.Store.YES, Field.Index.ANALYZED));
                    doc.Add(new Field("Content", content, Field.Store.NO, Field.Index.ANALYZED));

                    // _indexWriter.AddDocument(doc);
                    // 索引存在时更新，不存在时添加
                    _indexWriter.UpdateDocument(new Term("FileMark", fileMark), doc);

                    try
                    {
                        if (_finishCount % 500 == 0 || _finishCount == _totalCount)
                        {
                            try
                            {
                                // 索引刷新
                                _indexWriter.Optimize(10000);
                            }
                            catch (OutOfMemoryException ex)
                            {
                                log.Error(ex.Message, ex);

                                // 关闭并销毁索引写入器
                                CloseIndexWriter();

                                // 创建索引写入器
                                CreateIndexWriter();
                            }

                            // 手动GC
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(filePath + " -> " + ex.Message, ex);
                    }
                }
                msg.Append("，索引：" + taskMark.ConsumeTime + "秒");

                // 执行状态回调
                // taskInfo.Callback(msg.ToString(), CalcFinishRatio(_finishCount, taskInfo.TotalCount));
                _callback(msg.ToString(), CalcFinishRatio(_finishCount, _totalCount));

                log.Debug(msg);
            }
            catch (Exception ex)
            {
                log.Error(filePath + " -> " + ex.Message, ex);
            }
            finally
            {
                lock (locker)
                {
                    _finishCount++;
                }

                try
                {
                    // 标记当前任务完成，唤醒等待线程继续执行
                    taskInfo.ResetEvent.SetOne();
                }
                catch { }              
            }
        }
        #endregion

        #region 完成比例计算器
        /// <summary>
        /// 计算完成比例
        /// </summary>
        /// <param name="finishCount"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        private static double CalcFinishRatio(double finishCount, double totalCount)
        {
            return finishCount * 1.00F / totalCount * 1.00F * 100.00F;
        }
        #endregion

        #region 任务信息对象
        /// <summary>
        /// 任务信息
        /// </summary>
        class TaskInfo
        {
            /// <summary>
            /// 文件路径
            /// </summary>
            public string FilePath { get; set; }
            /// <summary>
            /// 多线程重置
            /// </summary>
            public MutipleThreadResetEvent ResetEvent { get; set; }
        }
        #endregion
    }
}
