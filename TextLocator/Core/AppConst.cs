using JiebaNet.Segmenter;
using System;
using System.IO;
using System.Text.RegularExpressions;
using TextLocator.Jieba;
using TextLocator.Util;

namespace TextLocator.Core
{
    /// <summary>
    /// App常量
    /// </summary>
    public class AppConst
    {
        /// <summary>
        /// 线程池最小数量
        /// </summary>
        public static readonly int THREAD_POOL_MIN_SIZE = int.Parse(AppUtil.ReadValue("AppConfig", "ThreadPoolMinSize", "8"));
        /// <summary>
        /// 线程池最大数量
        /// </summary>
        public static readonly int THREAD_POOL_MAX_SIZE = int.Parse(AppUtil.ReadValue("AppConfig", "ThreadPoolMaxSize", "24"));
        /// <summary>
        /// 应用目录
        /// </summary>
        public static readonly string APP_DIR = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// App.ini路径：_AppDir\\_AppName\\Index\\
        /// </summary>
        public static readonly string APP_INDEX_DIR = Path.Combine(APP_DIR, "Index");
        /// <summary>
        /// App.ini路径：_AppDir\\_AppName\\Index_Build\\
        /// </summary>
        public static readonly string APP_INDEX_BUILD_DIR = Path.Combine(APP_DIR, "Index_Build");
        /// <summary>
        /// 分词器
        /// new Lucene.Net.Analysis.Cn.ChineseAnalyzer();
        /// new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);// 用standardAnalyzer分词器
        /// </summary>
        public static readonly Lucene.Net.Analysis.Analyzer INDEX_ANALYZER = new JiebaAnalyzer(); //new Lucene.Net.Analysis.PanGuAnalyzer();
        /// <summary>
        /// 分割器
        /// </summary>
        public static readonly JiebaSegmenter INDEX_SEGMENTER = new JiebaSegmenter();

        /// <summary>
        /// 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
        /// 磁盘路径：Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(_AppIndexDir))
        /// 内存：new Lucene.Net.Store.RAMDirectory()
        /// </summary>
        public static readonly Lucene.Net.Store.FSDirectory INDEX_DIRECTORY = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(APP_INDEX_DIR));
        /// <summary>
        /// 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
        /// 磁盘路径：Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(_AppIndexDir))
        /// 内存：new Lucene.Net.Store.RAMDirectory()
        /// </summary>
        public static readonly Lucene.Net.Store.FSDirectory INDEX_BUILD_DIRECTORY = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(APP_INDEX_BUILD_DIR));

        /// <summary>
        /// 搜索最大值限制
        /// </summary>
        public static readonly int MAX_COUNT_LIMIT = int.Parse(AppUtil.ReadValue("AppConfig", "MaxCountLimit", "100"));

        /// <summary>
        /// 匹配特殊字符
        /// </summary>
        public static readonly Regex REGIX_SPECIAL_CHARACTER = new Regex("`|~|!|@|#|\\$|%|\\^|&|\\*|\\(|\\)|_|\\-|\\+|\\=|\\[|\\]|\\{|\\}|\\\\|\\||;|:|'|\"|,|\\<|\\.|\\>|\\/|\\?|");
        /// <summary>
        /// 匹配空白和换行
        /// </summary>
        public static readonly Regex REGIX_LINE_BREAKS_AND_WHITESPACE = new Regex(" |\r|\n|\\s");
        /// <summary>
        /// 匹配HTML和XML标签
        /// </summary>
        public static readonly Regex REGIX_TAG = new Regex("\\<.[^<>]*\\>");
        /// <summary>
        /// 匹配文件后缀
        /// </summary>
        public static readonly Regex REGIX_FILE_EXT = new Regex(@"^.+\.(" + FileTypeUtil.GetFileTypeExts("|") + ")$");
    }
}
