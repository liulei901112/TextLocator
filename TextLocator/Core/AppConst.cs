using System;
using System.IO;

namespace TextLocator.Core
{
    /// <summary>
    /// App常量
    /// </summary>
    public class AppConst
    {
        /// <summary>
        /// 应用目录
        /// </summary>
        public static readonly string APP_DIR = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// App.ini路径：_AppDir\\_AppName\\Index\\
        /// </summary>
        public static readonly string APP_INDEX_DIR = Path.Combine(APP_DIR, "Index");
        /// <summary>
        /// 分词器
        /// new Lucene.Net.Analysis.Cn.ChineseAnalyzer();
        /// new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);// 用standardAnalyzer分词器
        /// </summary>
        public static readonly Lucene.Net.Analysis.Analyzer INDEX_ANALYZER = new Lucene.Net.Analysis.PanGuAnalyzer();

        /// <summary>
        /// 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
        /// 磁盘路径：Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(_AppIndexDir))
        /// 内存：new Lucene.Net.Store.RAMDirectory()
        /// </summary>
        public static readonly Lucene.Net.Store.FSDirectory INDEX_DIRECTORY = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(APP_INDEX_DIR));
    }
}
