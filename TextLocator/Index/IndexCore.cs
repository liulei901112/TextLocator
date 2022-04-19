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
        /// <param name="msg">消息</param>
        /// <param name="percent">进度条比例，默认最大值</param>
        public delegate void CallbackStatus(string msg, double percent = AppConst.MAX_PERCENT);

        /// <summary>
        /// 锁对象
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// 错误数量
        /// </summary>
        private static volatile int _errorCount = 0;
        /// <summary>
        /// 完成数量
        /// </summary>
        private static volatile int _finishCount = 0;
        /// <summary>
        /// 文件总数
        /// </summary>
        private static volatile int _totalCount = 0;
        /// <summary>
        /// 索引函数
        /// </summary>
        private static volatile CallbackStatus _indexCallback;
        /// <summary>
        /// 搜索回调
        /// </summary>
        private static volatile CallbackStatus _searchCallback;

        /// <summary>
        /// 删除索引
        /// </summary>
        private static volatile Queue<int> _indexDeleted = new Queue<int>();

        #region 索引写入器
        /// <summary>
        /// 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
        /// * 但目录修改为10个目录随机分开写，防止索引太大OOM（思想上的解决方案，具体有待实现验证）
        /// </summary>
        private static volatile List<Lucene.Net.Index.IndexWriter> indexWriters = new List<Lucene.Net.Index.IndexWriter>();
        /// <summary>
        /// 索引写入目录（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
        /// 磁盘路径：FSDirectory.Open(new DirectoryInfo(_AppIndexDir))
        /// 内存：new RAMDirectory()
        /// * 索引目录和写入器对应
        /// </summary>
        private static volatile List<Lucene.Net.Store.FSDirectory> indexWriterDirs = new List<Lucene.Net.Store.FSDirectory>();

        /// <summary>
        /// 创建索引写入器
        /// </summary>
        /// <param name="areaId">区域ID（区域索引标识符）</param>
        /// <param name="create">是否是创建</param>
        /// <param name="index">索引写入器下标</param>
        private static void CreateIndexWriter(string areaId, bool create, int index = -1)
        {
            // 内部函数
            void BuiltIn(int subIndex, bool isNew = false)
            {
                // 索引子目录
                string appIndexDirSub = Path.Combine(AppConst.APP_INDEX_DIR, areaId, subIndex + "");
                if (!Directory.Exists(appIndexDirSub)) Directory.CreateDirectory(appIndexDirSub);

                // 如果目录下的文件为空，说明是新建
                if (Directory.GetFiles(appIndexDirSub).Length == 0)
                {
                    create = true;
                }

                // ---- 索引目录
                Lucene.Net.Store.FSDirectory indexWriterDir = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(appIndexDirSub), new Lucene.Net.Store.NativeFSLockFactory());
                if (isNew)
                {
                    indexWriterDirs.Add(indexWriterDir);
                }
                else
                {
                    indexWriterDirs[subIndex] = indexWriterDir;
                }               

                //  如果索引目录被锁定（比如索引过程中程序异常退出），则首先解锁
                //  Lucene.Net在写索引库之前会自动加锁，在Close的时候会自动解锁
                //  不能多线程执行，只能处理意外被永远锁定的情况
                if (Lucene.Net.Index.IndexWriter.IsLocked(indexWriterDir))
                {
                    // UnLock：强制解锁
                    Lucene.Net.Index.IndexWriter.Unlock(indexWriterDir);
                }

                // ---- 索引写入器
                // 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
                // 补充：使用IndexWriter打开Directory时会自动对索引库文件上锁
                Lucene.Net.Index.IndexWriter indexWriter = new Lucene.Net.Index.IndexWriter(
                    // 索引目录
                    indexWriterDir,
                    // 分词器
                    AppConst.INDEX_ANALYZER,
                    // 是否创建
                    create,
                    // 字段限制
                    Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

                // 设置Buffer内存上限，默认值16MB
                indexWriter.SetRAMBufferSizeMB(64);
                // 设置Buffer内存文档上线
                indexWriter.SetMaxBufferedDocs(1000);

                if (isNew)
                {
                    indexWriters.Add(indexWriter);
                }
                else
                {
                    indexWriters[subIndex] = indexWriter;
                }
            }

            if (index <= -1)
            {
                // 创建10个写入器
                for (int i = 0; i < 10; i++)
                {
                    BuiltIn(i, true);
                }                
            }
            else
            {
                BuiltIn(index);
            }
        }

        /// <summary>
        /// 关闭索引写入器
        /// <param name="index">索引写入器下标，-1标识全部</param>
        /// </summary>
        private static void CloseIndexWriter(int index = -1)
        {
            // 内部函数
            void BuiltIn(int subIndex)
            {
                Lucene.Net.Index.IndexWriter indexWriter = indexWriters[subIndex];
                if (indexWriter != null)
                {
                    // 销毁索引写入器
                    indexWriter.Dispose();
                }
                Lucene.Net.Store.FSDirectory indexWriterDir = indexWriterDirs[subIndex];
                if (indexWriterDir != null)
                {
                    indexWriterDir.Dispose();
                }
            }
            if (index <= -1)
            {
                for(int i = 0; i < indexWriters.Count; i++)
                {
                    BuiltIn(i);
                }
                indexWriters.Clear();
                indexWriterDirs.Clear();
            }
            else
            {
                BuiltIn(index);
            }
            
        }

        /// <summary>
        /// 区域索引标记
        /// </summary>
        /// <param name="areaId">区域ID，不允许为空</param>
        /// <returns></returns>
        private static string AreaIndexTag(string areaId)
        {
            // 不同区域，索引分开记录
            return areaId + "Index";
        }
        #endregion

        #region 创建或更新索引
        /// <summary>
        /// 创建或更新索引
        /// </summary>
        /// <param name="areaId">区域ID（区域索引标识符）</param>
        /// <param name="updateFilePaths">文件列表</param>
        /// <param name="deleteFilePaths">删除文件列表</param>
        /// <param name="isRebuild">是否重建，true表示重建，false表示更新</param>
        /// <param name="callback">消息回调</param>
        public static int CreateIndex(string areaId, List<string> updateFilePaths, List<string> deleteFilePaths, bool isRebuild, CallbackStatus callback)
        {
            // 不同区域，索引分开记录
            string areaIdIndex = AreaIndexTag(areaId);

            // 排重 => 排序
            updateFilePaths = ListUtil.Shuffle(updateFilePaths.Distinct().ToList());

            // 记录全局回调函数
            _indexCallback = callback;

            // 文件总数
            _totalCount = updateFilePaths.Count();

            // 每次初始化的时候完成数量都是0
            _finishCount = 0;

            // 每次初始化的时候错误数量都是0
            _errorCount = 0;

            // 判断是创建索引还是增量索引（如果索引目录不存在，重建）
            bool create = !Directory.Exists(Path.Combine(AppConst.APP_INDEX_DIR, areaId));
            // 入参为true，表示重建
            if (isRebuild)
            {
                create = isRebuild;
            }

            // 创建还是更新？
            if (create)
            {
                // 重建时，删除全部标记
                AppUtil.DeleteSection(areaIdIndex);
            }

            // -------- 以下4个函数调用顺序不能更改 --------

            // 1、-------- 创建索引写入器（创建索引任务执行前，创建写入器很关键）
            CreateIndexWriter(areaId, create);

            // 2、-------- 删除文件索引（更新ini文件的AreaIdIndex标记，便于更新方法检测时使用）
            DeleteFileIndex(areaId, deleteFilePaths);

            // 3、-------- 更新文件索引
            UpdateFileIndex(areaId, updateFilePaths, create);

            // 4、-------- 删除搜索时标记的索引
            DeleteFileIndexForSearch();

            lock (locker)
            {
                try
                {
                    foreach(Lucene.Net.Index.IndexWriter indexWriter in indexWriters)
                    {
                        // 索引优化
                        indexWriter.Optimize();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("索引优化错误：" + ex.Message, ex);
                }

                // 关闭索引写入器
                CloseIndexWriter();
            }

            // 手动GC
            AppCore.ManualGC();

            // 返回错误数量，调用者可根据总数和错误数量，知道完成实际有效的索引文件数量
            return _errorCount;
        }

        /// <summary>
        /// 更新文件索引
        /// </summary>
        /// <param name="areaId">区域ID（区域索引标识符）</param>
        /// <param name="updateFilePaths">更新文件列表</param>
        /// <param name="create">是否是创建</param>
        /// <returns></returns>
        private static void UpdateFileIndex(string areaId, List<string> updateFilePaths, bool create)
        {
            if (updateFilePaths.Count > 0)
            {
                using (MutipleThreadResetEvent resetEvent = new MutipleThreadResetEvent(_totalCount))
                {
                    // 遍历读取文件，并创建索引
                    for (int i = 0; i < _totalCount; i++)
                    {
                        string filePath = updateFilePaths[i];
                        // 忽略已存在索引的文件
                        if (SkipFile(areaId, create, filePath, resetEvent))
                        {
                            continue;
                        }
                        // 加入线程池
                        ThreadPool.QueueUserWorkItem(new WaitCallback(CreateIndexTask), new TaskInfo()
                        {
                            AreaId = areaId,
                            FilePath = filePath,
                            ResetEvent = resetEvent
                        });
                    }

                    // 等待所有线程结束
                    resetEvent.WaitAll();
                }
            }
            else
            {
                log.Info("需要更新索引的文件列表为空");
            }
        }

        /// <summary>
        /// 删除文件索引
        /// </summary>
        /// <param name="areaId">区域ID（区域索引标识符）</param>
        /// <param name="deleteFilePaths">删除文件列表</param>
        /// <returns></returns>
        private static void DeleteFileIndex(string areaId, List<string> deleteFilePaths)
        {
            if (deleteFilePaths.Count > 0)
            {
                // 不同区域，索引分开记录
                string areaIdIndex = AreaIndexTag(areaId);

                foreach (string filePath in deleteFilePaths)
                {
                    // 文件标记
                    string id = MD5Util.GetMD5Hash(filePath);
                    try
                    {
                        foreach (Lucene.Net.Index.IndexWriter indexWriter in indexWriters)
                        {
                            indexWriter.DeleteDocuments(new Lucene.Net.Index.Term("Id", id));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("删除索引失败：" + ex.Message, ex);
                    }

                    // 删除标记和缓存
                    AppUtil.WriteValue(areaIdIndex, filePath, null);
                }
            }
            else
            {
                log.Info("需要删除索引的文件列表为空");
            }
        }

        /// <summary>
        /// 删除文件索引ForSearch
        /// </summary>
        /// <returns></returns>
        private static void DeleteFileIndexForSearch()
        {
            if (_indexDeleted.Count > 0)
            {
                lock (_indexDeleted)
                {
                    while (_indexDeleted.Count > 0)
                    {
                        try
                        {
                            foreach (Lucene.Net.Index.IndexWriter indexWriter in indexWriters)
                            {
                                indexWriter.GetReader().DeleteDocument(_indexDeleted.Dequeue());
                            }                            
                        }
                        catch (Exception ex)
                        {
                            log.Error("删除索引失败：" + ex.Message, ex);
                        }
                    }
                }
            }
            else
            {
                log.Info("需要删除索引的标记队列为空");
            }
        }

        /// <summary>
        /// 忽略文件
        /// </summary>
        /// <param name="areaId">区域ID（区域索引标识符），不允许为空</param>
        /// <param name="create">是否是创建，true为创建、false为更新</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="resetEvent">多线程任务标记</param>
        private static bool SkipFile(string areaId, bool create, string filePath, MutipleThreadResetEvent resetEvent)
        {
            try
            {
                // 不同区域，索引分开记录
                string areaIdIndex = AreaIndexTag(areaId);

                FileInfo fileInfo = new FileInfo(filePath);
                // 当前文件修改时间
                string lastWriteTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                // 上次索引时文件修改时间标记
                string lastWriteTimeTag = AppUtil.ReadValue(areaIdIndex, filePath);

                // 非重建 && 文件已经被索引过
                bool isUpdate = !create;
                bool isExists = lastWriteTime.Equals(lastWriteTimeTag);
                // 文件修改时间不一致，说明文件已修改
                if (isUpdate && isExists)
                {
                    string skip = "跳过文件：" + filePath;

                    // 跳过的文件
                    if (_indexCallback != null)
                    {
                        _indexCallback(skip, CalcFinishRatio(_finishCount, _totalCount));
                    }

                    // 完成数量+1
                    Interlocked.Increment(ref _finishCount);

                    // 当前任务执行完成，唤醒等待线程继续执行
                    resetEvent.SetOne();

                    log.Debug(skip);
                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 创建索引任务方法
        /// </summary>
        /// <param name="obj">实际传入TaskInfo</param>
        private static void CreateIndexTask(object obj)
        {
            TaskInfo taskInfo = obj as TaskInfo;
            try
            {
                // 不同区域，索引分开记录
                string areaIdIndex = AreaIndexTag(taskInfo.AreaId);

                // 解析时间
                var taskMark = TaskTime.StartNew();

                // 文件路径
                string filePath = taskInfo.FilePath;

                // 文件信息
                FileInfo fileInfo = new FileInfo(filePath);
                // 文件名
                string fileName = fileInfo.Name;
                // 文件大小
                long fileSize = fileInfo.Length;
                // 创建时间
                string createTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                // 创建时间
                string updateTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

                // ---- 写入已索引标记 ----
                AppUtil.WriteValue(areaIdIndex, filePath, fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.ffff"));

                // 根据文件路径获取文件类型（自定义文件类型分类）
                FileType fileType = FileTypeUtil.GetFileType(filePath);

                // 截取文件路径
                string subFilePath = filePath;
                if (subFilePath.Length > 65)
                {
                    subFilePath = filePath.Substring(0, 30) + "......" + filePath.Substring(filePath.Length - 30);
                }

                StringBuilder msg = new StringBuilder("[" + _finishCount * 1.0F + "/" + _totalCount + "] => 引擎：" + (int)fileType + "，文件：" + subFilePath);

                // 文件内容
                string content = FileInfoServiceFactory.GetFileContent(filePath);

                msg.Append("，解析：" + taskMark.ConsumeTime);
                // 判断文件内容
                if (!string.IsNullOrEmpty(content))
                {
                    // 索引ID（文件索引唯一标识符）
                    string id = MD5Util.GetMD5Hash(filePath);

                    // 索引时间
                    taskMark = TaskTime.StartNew();

                    lock (locker)
                    {
                        // 创建索引Doc对象
                        Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
                        // -------- 文件索引ID（索引唯一标识符）
                        doc.Add(new Lucene.Net.Documents.Field("Id", id, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                        // -------- 不分词建索引
                        doc.Add(new Lucene.Net.Documents.Field("FileType", fileType.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                        doc.Add(new Lucene.Net.Documents.Field("FileSize", fileSize + "", Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                        doc.Add(new Lucene.Net.Documents.Field("CreateTime", createTime, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                        doc.Add(new Lucene.Net.Documents.Field("UpdateTime", updateTime, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                        
                        // -------- ANALYZED分词建索引
                        doc.Add(new Lucene.Net.Documents.Field("FileName", fileName, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                        doc.Add(new Lucene.Net.Documents.Field("FilePath", filePath, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                        
                        // -------- 内容处理（默认有分页----1----，content需要替换，但是预览需要保留，同时content只分词不存储，preview只存储不分词）
                        doc.Add(new Lucene.Net.Documents.Field("Content", AppConst.REGEX_CONTENT_PAGE.Replace(content, ""), Lucene.Net.Documents.Field.Store.NO, Lucene.Net.Documents.Field.Index.ANALYZED));
                        doc.Add(new Lucene.Net.Documents.Field("Preview", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                        // 执行删除、添加逻辑
                        AddDocument(taskInfo.AreaId, filePath, doc);
                    }
                    msg.Append("，索引：" + taskMark.ConsumeTime);
                }
                else
                {
                    msg.Append("，返回内容为空不创建索引。。。");
                    // 错误数量+1
                    Interlocked.Increment(ref _errorCount);
                }

                // 执行状态回调
                if (_indexCallback != null)
                {
                    _indexCallback(msg.ToString(), CalcFinishRatio(_finishCount, _totalCount));
                }

                log.Debug(msg);
            }
            catch (Exception ex)
            {
                log.Error(taskInfo.FilePath + " -> 创建索引错误：" + ex.Message, ex);
            }
            finally
            {
                // 完成数量+1
                Interlocked.Increment(ref _finishCount);

                // 当前任务执行完成，唤醒等待线程继续执行
                taskInfo.ResetEvent.SetOne();
            }
        }

        /// <summary>
        /// 添加文件索引Doc
        /// </summary>
        /// <param name="areaId">区域ID（区域索引标识符），不允许为null</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="doc">文件索引Doc对象</param>
        private static void AddDocument(string areaId, string filePath, Lucene.Net.Documents.Document doc)
        {
            // -------- 获取索引写入器
            // 随机索引写入器
            int index = new Random().Next(10);

            try
            {
                // 文件标记
                string fileMark = MD5Util.GetMD5Hash(filePath);

                indexWriters[index].UpdateDocument(new Lucene.Net.Index.Term("FileMark", fileMark), doc);
            }
            catch(Exception ex)
            {
                log.Error(filePath + " -> " + ex.Message + " => 重启索引写入器！", ex);


                // 关闭索引写入器（区域ID + 写入器索引）
                CloseIndexWriter(index);

                // 创建索引写入器（区域ID + 更新 + 写入器索引）
                CreateIndexWriter(areaId, false, index);

                log.Debug(filePath + " -> 重新执行添加操作");

                AddDocument(areaId, filePath, doc);
            }
        }
        #endregion

        #region 关键词搜索
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="param">搜索参数</param>
        /// <returns></returns>
        public static Entity.SearchResult Search(Entity.SearchParam param, CallbackStatus callback = null)
        {
            _searchCallback = callback;

            // 定义搜索结果列表
            List<Entity.FileInfo> fileInfos = new List<Entity.FileInfo>();

            // 开始时间标记
            var taskMark = TaskTime.StartNew();

            // 搜索目录列表
            // 索引读取目录（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
            // 磁盘路径：FSDirectory.Open(new DirectoryInfo(_AppIndexDir))
            // 内存：new RAMDirectory()
            List<Lucene.Net.Store.FSDirectory> directorys = new List<Lucene.Net.Store.FSDirectory>();
            // 搜索器列表
            List<Lucene.Net.Search.IndexSearcher> searchers = new List<Lucene.Net.Search.IndexSearcher>();

            // 构造全部搜索区索引路径
            foreach(Entity.AreaInfo areaInfo in AreaUtil.GetEnableAreaInfoList())
            {
                for(int i = 0; i < 10; i++)
                {
                    try
                    {
                        string subDir = Path.Combine(AppConst.APP_INDEX_DIR, areaInfo.AreaId, i + "");
                        log.Debug("搜索缩影路径：" + subDir);
                        Lucene.Net.Store.FSDirectory directory = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(subDir), new Lucene.Net.Store.NoLockFactory());
                        directorys.Add(directory);
                        searchers.Add(new Lucene.Net.Search.IndexSearcher(directory, true));
                    }
                    catch (Exception ex)
                    {
                        log.Error("搜索器初始化失败：" + ex.Message, ex);
                    }
                }
            }
            // 并行多搜索器（搜索结果去重合并）
            Lucene.Net.Search.ParallelMultiSearcher parallelMultiSearcher = new Lucene.Net.Search.ParallelMultiSearcher(searchers.ToArray());
            try
            {
                // 搜索域加权
                Dictionary<string, float> boosts = new Dictionary<string, float>();
                boosts["Content"] = 1.2f;
                boosts["FileName"] = 1.0f;

                // 搜索域列表
                List<string> fields = new List<string>();
                fields.Add("FileName");

                if (!param.IsOnlyFileName)
                {
                    fields.Add("Content");
                }

                // 查询转换器
                Lucene.Net.QueryParsers.QueryParser parser =
                    new Lucene.Net.QueryParsers.MultiFieldQueryParser(
                        // Lucence版本
                        Lucene.Net.Util.Version.LUCENE_30,
                        // 搜索域列表
                        fields.ToArray(),
                        // 分析仪
                        AppConst.INDEX_ANALYZER, 
                        // 权重
                        boosts);

                // 复合查询
                Lucene.Net.Search.BooleanQuery boolQuery = new Lucene.Net.Search.BooleanQuery();

                // 遍历关键词列表
                string text = "";
                string tag = "分词";
                foreach (string keyword in param.Keywords)
                {
                    text += keyword + ",";
                    // 正则
                    if (AppConst.REGEX_SUPPORT_WILDCARDS.IsMatch(keyword))
                    {
                        tag = "正则";
                        // 文件名
                        RegexQuery regexFileName = new RegexQuery(new Lucene.Net.Index.Term("FileName", keyword));
                        boolQuery.Add(regexFileName, Lucene.Net.Search.Occur.SHOULD);

                        if (!param.IsOnlyFileName)
                        {
                            // 文件内容
                            RegexQuery regexContentQuery = new RegexQuery(new Lucene.Net.Index.Term("Content", keyword));
                            boolQuery.Add(regexContentQuery, Lucene.Net.Search.Occur.SHOULD);
                        }                        
                    }
                    // 常规
                    else
                    {
                        Lucene.Net.Search.Query query = parser.Parse(Lucene.Net.QueryParsers.QueryParser.Escape(keyword));
                        boolQuery.Add(query, param.IsMatchWords ? Lucene.Net.Search.Occur.MUST : Lucene.Net.Search.Occur.SHOULD);
                    }
                }
                text = text.Substring(0, text.Length - 1);
                log.Debug("搜索关键词：（" + text + "）, 文件类型：" + param.FileType);

                // 文件类型筛选（文件类型为全部时，则为空）
                Lucene.Net.Search.TermsFilter filter = null;
                if (!string.IsNullOrEmpty(param.FileType))
                {
                    filter = new Lucene.Net.Search.TermsFilter();
                    filter.AddTerm(new Lucene.Net.Index.Term("FileType", param.FileType));
                }
                log.Debug("组合搜索条件：" + boolQuery.ToString());

                // 排序（true表示降序，false表示升序）
                Lucene.Net.Search.Sort sort = new Lucene.Net.Search.Sort();
                switch (param.SortType)
                {
                    case SortType.默认排序: break;
                    case SortType.从远到近:
                        sort.SetSort(new Lucene.Net.Search.SortField("UpdateTime", Lucene.Net.Search.SortField.STRING_VAL, false));
                        break;
                    case SortType.从近到远:
                        sort.SetSort(new Lucene.Net.Search.SortField("UpdateTime", Lucene.Net.Search.SortField.STRING_VAL, true));
                        break;
                    case SortType.从小到大:
                        sort.SetSort(new Lucene.Net.Search.SortField("FileSize", Lucene.Net.Search.SortField.INT, false));
                        break;
                    case SortType.从大到小:
                        sort.SetSort(new Lucene.Net.Search.SortField("FileSize", Lucene.Net.Search.SortField.INT, true));
                        break;
                }

                // 查询数据分页
                Lucene.Net.Search.TopFieldDocs topDocs = parallelMultiSearcher.Search(boolQuery, filter, param.PageIndex * param.PageSize, sort);
                // 结果数组
                Lucene.Net.Search.ScoreDoc[] scores = topDocs.ScoreDocs;

                // 查询到的条数
                int totalHits = topDocs.TotalHits;

                // 索引文档对象
                Lucene.Net.Documents.Document doc;
                // 显示文件信息
                Entity.FileInfo fileInfo;

                // 计算显示数据
                int start = (param.PageIndex - 1) * param.PageSize;
                int end = param.PageSize * param.PageIndex;
                if (end > totalHits) end = totalHits;
                // 文件不存在
                int deleteCount = 0;
                // 获取并显示列表
                for (int i = start; i < end; i++)
                {
                    // 该文件的在索引里的文档号,Doc是该文档进入索引时Lucene的编号，默认按照顺序编的
                    int docId = scores[i].Doc;
                    // 获取文档对象
                    doc = parallelMultiSearcher.Doc(docId);// reader.Document(docId);

                    Lucene.Net.Documents.Field fileTypeField = doc.GetField("FileType");
                    Lucene.Net.Documents.Field fileNameField = doc.GetField("FileName");
                    Lucene.Net.Documents.Field filePathField = doc.GetField("FilePath");
                    Lucene.Net.Documents.Field fileSizeField = doc.GetField("FileSize");
                    Lucene.Net.Documents.Field createTimeField = doc.GetField("CreateTime");
                    Lucene.Net.Documents.Field updateTimeField = doc.GetField("UpdateTime");
                    Lucene.Net.Documents.Field previewField = doc.GetField("Preview");

                    // 判断本地是否存在该文件，存在则在检索结果栏里显示出来
                    if (!File.Exists(filePathField.StringValue))
                    {
                        // 因为查询设置了 readOnly ，所以知识标记该文档索引需要删除，并跳过显示。如果需要直接删除，可调用一下两行代码。但是如果索引正在写入，就会抛出异常。
                        // reader.DeleteDocument(docId);
                        // reader.Commit();
                        lock (_indexDeleted)
                        {
                            // 记录为需要删除的索引，索引更新任务执行结束，执行索引删除操作
                            _indexDeleted.Enqueue(docId);
                            deleteCount++;
                        }
                        continue;
                    }

                    log.Debug("索引：" + fileNameField.StringValue + " => " + filePathField.StringValue + " ， " + fileSizeField.StringValue + " , " + updateTimeField.StringValue);

                    // 处理预览内容

                    // 构造显示文件信息
                    fileInfo = new Entity.FileInfo()
                    {
                        FilePath = filePathField.StringValue,
                        FileName = fileNameField.StringValue,
                        Preview = previewField.StringValue,
                        FileSize = long.Parse(fileSizeField.StringValue),
                        CreateTime = createTimeField.StringValue,
                        UpdateTime = updateTimeField.StringValue,
                        Keywords = param.Keywords
                    };

                    try
                    {
                        fileInfo.FileType = (FileType)System.Enum.Parse(typeof(FileType), fileTypeField.StringValue);
                    }
                    catch
                    {
                        fileInfo.FileType = FileType.TXT文档;
                    }
                    fileInfos.Add(fileInfo);
                }

                string msg = string.Format("检索完成。{0}：( {1} )，结果：{2}个符合条件的结果 (第 {3} 页)，耗时：{4}。", tag, (text.Length > 50 ? text.Substring(0, 50) + "..." : text), totalHits - deleteCount, param.PageIndex, taskMark.ConsumeTime);
                log.Debug(msg);
                if (_searchCallback != null)
                    _searchCallback(msg);

                // 返回查询结果
                return new Entity.SearchResult()
                {
                    Total = totalHits,
                    Results = fileInfos
                };
            }
            catch (Exception ex)
            {
                log.Error("搜索错误：param => " + Newtonsoft.Json.JsonConvert.SerializeObject(param) + ", error => " + ex.Message, ex);
                return null;
            }
            finally
            {
                foreach(var searcher in searchers)
                {
                    try
                    {
                        if (searcher != null)
                        {
                            searcher.Dispose();
                        }
                    }
                    catch { }
                }
                foreach(var dir in directorys)
                {
                    try
                    {
                        if (dir != null)
                        {
                            dir.Dispose();
                        }
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 获取关键词词频
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="autoNewLine">自动换行</param>
        /// <returns></returns>
        public static string GetKeywordFrequency(Entity.FileInfo fileInfo, bool autoNewLine = true)
        {
            return GetKeywordFrequency(fileInfo.Keywords, fileInfo.FileName, fileInfo.Preview, autoNewLine);
        }

        /// <summary>
        /// 获取关键词词频
        /// </summary>
        /// <param name="keywords">关键词列表</param>
        /// <param name="fileName">文件名</param>
        /// <param name="preview">文件内容</param>
        /// <param name="autoNewLine">自动换行</param>
        /// <returns></returns>
        private static string GetKeywordFrequency(List<string> keywords, string fileName, string preview, bool autoNewLine = true)
        {
            try
            {
                // 获取内容（预览内容替换----\d+----）
                string content = AppConst.REGEX_CONTENT_PAGE.Replace(preview, "");

                TaskTime taskTime = TaskTime.StartNew();
                // 定义词频词典
                Dictionary<string, int> frequencyDic = new Dictionary<string, int>();

                // 遍历关键词
                foreach (string keyword in keywords)
                {
                    // 声明正则
                    Regex regex = new Regex(keyword);
                    // 匹配文件名
                    Match matchName = regex.Match(fileName), matchContent = regex.Match(content);
                    // 匹配内容
                    int matchNameCount = 0, matchContentCount = 0;
                    // 文件名匹配成功
                    if (matchName.Success)
                    {
                        // 获取匹配次数
                        matchNameCount = regex.Matches(fileName).Count;
                    }
                    // 文件内容匹配成功
                    if (matchContent.Success)
                    {
                        // 获取匹配次数
                        matchContentCount = regex.Matches(content).Count;
                    }
                    // 匹配数量合并
                    int count = matchNameCount + matchContentCount;
                    // 匹配次数大于才是有效值
                    if (count > 0)
                    {
                        frequencyDic[keyword] = count;
                    }
                }
                StringBuilder builder = new StringBuilder();
                // 获取匹配词列表
                List<string> frequencyKeyList = frequencyDic.Keys.ToList();
                for (int k = 0; k < frequencyKeyList.Count; k++)
                {
                    builder.Append(string.Format("{0}：{1}，", frequencyKeyList[k], frequencyDic[frequencyKeyList[k]]));
                    // 自动换行 && （下标不是0 && 每4次 && 下标不是最大）
                    if (autoNewLine && k > 0 && k % 8 == 0 && k < frequencyKeyList.Count - 1)
                    {
                        builder.Append("\r\n");
                    }
                }
                string text = builder.ToString();
                if (text.EndsWith("，"))
                {
                    text = text.Substring(0, text.Length - 1);
                }
                log.Debug(fileName + " -> 词频统计耗时：" + taskTime.ConsumeTime + " 统计词频：" + text);
                return text;
            }
            catch (Exception ex)
            {
                log.Error("获取关键词频率失败：" + ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// 获取内容缩略
        /// </summary>
        /// <param name="keywords">关键词列表</param>
        /// <param name="preview">预览内容</param>
        /// <returns></returns>
        public static string GetContentBreviary(Entity.FileInfo fileInfo)
        {
            // 获取内容
            string content = AppConst.REGEX_CONTENT_PAGE.Replace(fileInfo.Preview, "");
            // 缩略信息
            string breviary = AppConst.REGEX_LINE_BREAKS_AND_WHITESPACE.Replace(content, "");

            int min = 0;
            int max = breviary.Length;
            int subLen = 130;
            try
            {
                // 内部子方法
                string ContentBreviary(int index)
                {
                    int startIndex = index - subLen / 2;
                    int endIndex = index + subLen / 2;

                    // 顺序不能乱
                    if (startIndex < min) startIndex = min;
                    if (endIndex > max) endIndex = max;
                    if (startIndex > endIndex) startIndex = endIndex - subLen;
                    if (startIndex < min) startIndex = min;
                    if (startIndex + endIndex < subLen) endIndex = endIndex + subLen - (startIndex + endIndex);
                    if (endIndex > max) endIndex = max;

                    breviary = breviary.Substring(startIndex, endIndex - startIndex);

                    if (startIndex > min) breviary = "..." + breviary;
                    if (endIndex < max) breviary = breviary + "...";
                    return breviary;
                }

                foreach (string keyword in fileInfo.Keywords)
                {
                    if (string.IsNullOrEmpty(keyword)) continue;
                    // 关键词是正则表达式
                    if (AppConst.REGEX_SUPPORT_WILDCARDS.IsMatch(keyword))
                    {
                        Regex regex = new Regex(keyword, RegexOptions.IgnoreCase);
                        Match matches = regex.Match(content);
                        if (matches.Success)
                        {
                            return ContentBreviary(matches.Index);
                        }
                    }
                    else
                    {
                        // 可能包含多个keyword,做遍历查找                    
                        int index = breviary.IndexOf(keyword, 0, StringComparison.CurrentCultureIgnoreCase);
                        if (index != -1)
                        {
                            return ContentBreviary(index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("获取摘要信息失败：" + ex.Message, ex);
            }

            // 默认预览内容
            if (breviary.Length > AppConst.FILE_CONTENT_SUB_LENGTH)
            {
                breviary = breviary.Substring(0, AppConst.FILE_CONTENT_SUB_LENGTH) + "...";
            }
            return breviary;
        }
        #endregion

        #region 任务信息对象
        /// <summary>
        /// 任务信息
        /// </summary>
        class TaskInfo
        {
            /// <summary>
            /// 区域ID（区域唯一标识符）
            /// </summary>
            public string AreaId { get; set; }
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

        #region 索引完成比例计算器
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
    }
}
