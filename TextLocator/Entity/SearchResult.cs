using System.Collections.Generic;

namespace TextLocator.Entity
{
    /// <summary>
    /// 搜索结果
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// 结果总数
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 结果列表
        /// </summary>
        public List<FileInfo> Results { get; set; }
    }
}
