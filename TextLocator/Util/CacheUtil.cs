using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Util
{
    /// <summary>
    /// 简单缓存工具类
    /// </summary>
    public class CacheUtil
    {
        //缓存容器 
        private static Dictionary<string, object> cacheDic = new Dictionary<string, object>();

        /// <summary>
        /// 添加缓存
        /// </summary>
        public static void Add(string key, object value)
        {
            cacheDic[key] = value;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public static T Get<T>(string key)
        {
            try
            {
                return (T)cacheDic[key];
            } catch { }
            return default(T);
        }

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exsits(string key)
        {
            return cacheDic.ContainsKey(key);
        }
    }
}
