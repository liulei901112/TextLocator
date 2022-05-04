using System.Collections.Generic;
using TextLocator.Enums;

namespace TextLocator.Entity
{
    /// <summary>
    /// 文件信息
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// 文件类型
        /// </summary>
        public FileType FileType { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// 文件内容（预览内容，匹配词计算时需要先替换----\d+----）
        /// </summary>
        public string Preview { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string UpdateTime { get; set; }

        // -------- 查询参数携带回传 --------
        /// <summary>
        /// 关键词
        /// </summary>
        private List<string> keywords = new List<string>();
        public List<string> Keywords { get => keywords; set => keywords = value; }
        /// <summary>
        /// 词频统计
        /// </summary>
        public int MatchCount { get; set; }
        /// <summary>
        /// 搜索域
        /// </summary>
        private SearchRegion searchRegion = SearchRegion.文件名和内容;
        public SearchRegion SearchRegion { get => searchRegion; set => searchRegion = value; }
    }
}
