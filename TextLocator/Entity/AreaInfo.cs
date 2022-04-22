using System.Collections.Generic;
using TextLocator.Enums;

namespace TextLocator.Entity
{
    /// <summary>
    /// 区域信息
    /// </summary>
    public class AreaInfo
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 区域索引
        /// </summary>
        public string AreaId { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 区域文件夹路径
        /// </summary>
        public List<string> AreaFolders { get; set; }
        /// <summary>
        /// 区域文件类型
        /// </summary>
        public List<FileType> AreaFileTypes { get; set; }
    }
}
