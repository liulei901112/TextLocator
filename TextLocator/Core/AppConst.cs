using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using System;
using System.Diagnostics;
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
        public static int THREAD_POOL_MIN_SIZE = int.Parse(AppUtil.ReadValue("ThreadPool", "MinSize", "32"));
        /// <summary>
        /// 线程池最大数量
        /// </summary>
        public static int THREAD_POOL_MAX_SIZE = int.Parse(AppUtil.ReadValue("ThreadPool", "MaxSize", "64"));
        /// <summary>
        /// 结果列表分页条数
        /// </summary>
        public static int MRESULT_LIST_PAGE_SIZE = int.Parse(AppUtil.ReadValue("AppConfig", "ResultListPageSize", "100"));
        /// <summary>
        /// 文件读取超时时间，单位：秒
        /// </summary>
        public static int FILE_READ_TIMEOUT = int.Parse(AppUtil.ReadValue("AppConfig", "FileReadTimeout", "600"));
        /// <summary>
        /// 文件大小限制
        /// </summary>
        public static int FILE_SIZE_LIMIT = int.Parse(AppUtil.ReadValue("AppConfig", "FileSizeLimit", "200000000"));
        /// <summary>
        /// 缓存池容量
        /// </summary>
        public static int CACHE_POOL_CAPACITY = int.Parse(AppUtil.ReadValue("AppConfig", "CachePoolCapacity", "100000"));
        /// <summary>
        /// 启用索引更新任务，默认启用
        /// </summary>
        public static bool ENABLE_INDEX_UPDATE_TASK = bool.Parse(AppUtil.ReadValue("AppConfig", "EnableIndexUpdateTask", "True"));
        /// <summary>
        /// 索引更新任务间隔时间，单位：分
        /// </summary>
        public static int INDEX_UPDATE_TASK_INTERVAL = int.Parse(AppUtil.ReadValue("AppConfig", "IndexUpdateTaskInterval", "10"));

        /// <summary>
        /// AppName
        /// </summary>
        public static readonly string APP_NAME = Process.GetCurrentProcess().ProcessName.Replace(".exe", "");
        /// <summary>
        /// AppDir
        /// </summary>
        public static readonly string APP_DIR = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// AppDataDir：C:\Users\${User}\AppData\Roaming\
        /// </summary>
        public static readonly string APP_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        /// <summary>
        /// 索引路径：AppDir\Index\
        /// </summary>
        public static readonly string APP_INDEX_DIR = Path.Combine(APP_DIR, "Index");
        /// <summary>
        /// 分词器
        /// new Lucene.Net.Analysis.Cn.ChineseAnalyzer();
        /// new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);// 用standardAnalyzer分词器
        /// </summary>
        public static readonly Analyzer INDEX_ANALYZER = new JiebaAnalyzer(); //new Lucene.Net.Analysis.PanGuAnalyzer();
        /// <summary>
        /// 分割器
        /// </summary>
        public static readonly JiebaSegmenter INDEX_SEGMENTER = new JiebaSegmenter();

        /// <summary>
        /// 匹配Lucene.NET内置关键词
        /// </summary>
        public static readonly Regex REGEX_BUILT_IN_SYMBOL = new Regex("AND|OR|NOT|\\&\\&|\\|\\|");
        /// <summary>
        /// 匹配支持的通配符
        /// </summary>
        public static readonly Regex REGEX_SUPPORT_WILDCARDS = new Regex("\\+|\\-|\\||\\!|\\(|\\)|\\{|\\}|\\[|\\]|\\^|\"|\\~|\\*|\\?|\\:|\\/"); 
        /// <summary>
        /// 匹配空白和换行
        /// </summary>
        public static readonly Regex REGEX_LINE_BREAKS_AND_WHITESPACE = new Regex(@"  |\r\r|\n\n|┄|\. \. \. |\.\.\.|\s");
        /// <summary>
        /// 匹配HTML和XML标签
        /// </summary>
        public static readonly Regex REGEX_TAG = new Regex("\\<.[^<>]*\\>");
        /// <summary>
        /// 匹配排除关键词
        /// </summary>
        public static readonly Regex REGEX_EXCLUDE_KEYWORD = new Regex(@"(\$RECYCLE|360REC|SYSTEM|TEMP|SYSTEM VOLUME INFOMATION|\{(.*)\})");
        /// <summary>
        /// 匹配开始字符
        /// </summary>
        public static readonly Regex REGEX_START_WITH = new Regex(@"^(\`|\$|\~|\.)");
        /// <summary>
        /// 匹配内容分页符
        /// </summary>
        public static readonly Regex REGEX_CONTENT_PAGE = new Regex(@"----\d+----");

        /// <summary>
        /// 索引写入器
        /// </summary>
        public const int INDEX_PARTITION_COUNT = 5;
        /// <summary>
        /// 比例最小值
        /// </summary>
        public const int MIN_PERCENT = 0;
        /// <summary>
        /// 比例最大值
        /// </summary>
        public const int MAX_PERCENT = 100;
        /// <summary>
        /// 文件内容缩略信息截取值
        /// </summary>
        public const int FILE_CONTENT_SUB_LENGTH = 120;
        /// <summary>
        /// 文件词频数量限制
        /// </summary>
        public const int FILE_MATCH_COUNT_LIMIT = 800;
        /// <summary>
        /// 文件预览长度限制
        /// </summary>
        public const int FILE_PREVIEW_LEN_LIMIT = 1000000;
        /// <summary>
        /// 区域配置
        /// </summary>
        public const string AREA_CONFIG_KEY = "AreaConfig";
    }
}
