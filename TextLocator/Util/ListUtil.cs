using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Util
{
    /// <summary>
    /// list列表工具类
    /// </summary>
    internal class ListUtil
    {
        /// <summary>
        /// 洗牌（随机乱序）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static List<T> Shuffle<T>(List<T> list)
        {
            Random random = new Random();
            List<T> newList = new List<T>();
            foreach (var item in list)
            {
                newList.Insert(random.Next(newList.Count), item);
            }
            return newList;
        }
    }
}
