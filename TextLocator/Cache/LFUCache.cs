using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Cache
{

    /// <summary>
    /// LFU（Least Frequently Used）缓存机制
    /// 从数据集中，挑选最不经常使用的数据淘汰。
    /// </summary>
    public class LFUCache
    {
        /// <summary>
        /// 数据键值对
        /// </summary>
        private Dictionary<string, LFUCacheEntity> dataDic;
        /// <summary>
        /// 缓存频率数据节点
        /// </summary>
        private Dictionary<int, LinkedList<LFUCacheEntity>> frequenNodeListDic;
        /// <summary>
        /// 缓存容量大小
        /// </summary>
        private int _capacity;
        /// <summary>
        /// 最小频率
        /// </summary>
        private int _minFreq;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">缓存池容量</param>
        public LFUCache(int capacity)
        {
            _capacity = capacity;
            _minFreq = 0;

            dataDic = new Dictionary<string, LFUCacheEntity>(capacity);
            frequenNodeListDic = new Dictionary<int, LinkedList<LFUCacheEntity>>();

            frequenNodeListDic.Add(0, new LinkedList<LFUCacheEntity>());
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">接收数据类型</typeparam>
        /// <param name="key">缓存Key</param>
        /// <returns>缓存Value</returns>
        public T Get<T>(string key)
        {
            // 验证缓存是否存在
            if (!Exists(key))
                return default(T);

            // 获取缓存值
            var value = dataDic[key].Value;
            // 重新写入，频率计数器+1
            Put(key, value);
            try
            {
                // 返回Value
                return (T)value;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        public void Put(string key, object value)
        {
            // 如果容量为0，则返回
            if (_capacity == 0)
                return;
            
            // 构造新的缓存对象
            var newCacheData = new LFUCacheEntity { Key = key, Value = value, Frequen = 0 };
            
            // 缓存已存在
            if (dataDic.ContainsKey(key))
            {
                // 缓存数据对象
                var cacheEntity = dataDic[key];

                // 旧的计数器
                var oldFrequen = cacheEntity.Frequen;
                // 旧的计数器节点列表
                var oldFrequenNodeList = frequenNodeListDic[oldFrequen];
                // 从缓存频率数据节点
                oldFrequenNodeList.Remove(cacheEntity);

                // 频率计数器+1
                var newFrequen = oldFrequen + 1;
                // 缓存频率数据节点不存在
                if (!frequenNodeListDic.ContainsKey(newFrequen))
                {
                    // 新频率添加节点列表
                    frequenNodeListDic.Add(newFrequen, new LinkedList<LFUCacheEntity>());
                }

                // 设置新缓存频率计数器
                newCacheData.Frequen = newFrequen;
                // 缓存频率数据节点添加新缓存
                frequenNodeListDic[newFrequen].AddLast(newCacheData);
                // 数据缓存对象设置新缓存
                dataDic[key] = newCacheData;

                // 缓存频率数据节点存在缓存 && 节点数据为0
                if (frequenNodeListDic.ContainsKey(_minFreq) && frequenNodeListDic[_minFreq].Count == 0) {
                    // 记录访问频率
                    _minFreq = newFrequen;
                }
                return;
            }

            // 缓存池 超出容量
            if (_capacity <= dataDic.Count)
            {
                // 根据记录的访问频率获取需要删除的节点列表
                var deleteNodeList = frequenNodeListDic[_minFreq];
                // 获取需要删除的节点，节点列表第一个元素
                var deleteFirstNode = deleteNodeList.First;
                // 删除节点列表第一个元素
                deleteNodeList.RemoveFirst();
                // 数据缓存吃删除第一个元素对应的Key
                dataDic.Remove(deleteFirstNode.Value.Key);
            }

            // 缓存频率数据节点最后添加新缓存
            frequenNodeListDic[0].AddLast(newCacheData);
            // 数据缓存添加新缓存
            dataDic.Add(key, newCacheData);
            // 频率计数器归零
            _minFreq = 0;
        }

        /// <summary>
        /// 是否存在缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            return dataDic.ContainsKey(key);
        }
    }
}
