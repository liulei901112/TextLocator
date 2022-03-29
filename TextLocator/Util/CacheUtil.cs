using System.Collections.Generic;
using TextLocator.Cache;
using TextLocator.Core;

namespace TextLocator.Util
{
    /// <summary>
    /// 简单缓存工具类
    /// </summary>
    public class CacheUtil
    {
        /// <summary>
        /// LFU缓存
        /// </summary>
        private static LFUCache _cache;

        static CacheUtil()
        {
            _cache = new LFUCache(AppConst.CACHE_POOL_CAPACITY);
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        public static void Add(string key, object value)
        {
            _cache.Put(key, value);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public static T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exsits(string key)
        {
            return _cache.Exists(key);
        }
    }
}
