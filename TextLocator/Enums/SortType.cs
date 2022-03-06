using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Enums
{
    /// <summary>
    /// 排序类型
    /// </summary>
    public enum SortType
    {
        /// <summary>
        /// 默认排序
        /// </summary>
        默认排序 = 0,
        /// <summary>
        /// 创建时间正序
        /// </summary>
        从远到近 = 1,
        /// <summary>
        /// 创建时间倒叙
        /// </summary>
        从近到远 = 2,
        /// <summary>
        /// 文件大小正序
        /// </summary>
        从小到大 = 3,
        /// <summary>
        /// 文件大小倒叙
        /// </summary>
        从大到小 = 4
    }
}
