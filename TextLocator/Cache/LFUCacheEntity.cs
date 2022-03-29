using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Cache
{
    /// <summary>
    /// LFU缓存对象
    /// </summary>
    public class LFUCacheEntity
    {
        /// <summary>
        /// 缓存Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 缓存Value
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 缓存使用频率计数器
        /// </summary>
        public int Frequen { get; set; }
    }
}
