using System.Collections.Generic;

namespace TextLocator.Cache
{

    /// <summary>
    /// LFU（Least Frequently Used）缓存机制
    /// 从数据集中，挑选最不经常使用的数据淘汰。
    /// </summary>
    public class LFUCache
    {
        private Dictionary<string, Node> dict;
        private Dictionary<int, LinkedList<Node>> dictFrequenNodeList;
        private int _capacity;
        private int _minFreq;

        public LFUCache(int capacity)
        {
            _capacity = capacity;
            _minFreq = 0;

            dict = new Dictionary<string, Node>(capacity);
            dictFrequenNodeList = new Dictionary<int, LinkedList<Node>>();

            dictFrequenNodeList.Add(0, new LinkedList<Node>());
        }
        public T Get<T>(string key)
        {
            if (!Exists(key))
            {
                return default(T);
            }

            var value = dict[key].Value;
            Put(key, value);
            try
            {
                return (T)value;
            }
            catch
            {
                return default(T);
            }
        }

        public bool Exists(string key)
        {
            if (dict.TryGetValue(key, out Node value))
            {
                return value != null;
            }
            return false;
        }

        public void Put(string key, object value)
        {
            if (_capacity == 0)
                return;

            var newNode = new Node(key, value);

            if (dict.ContainsKey(key))
            {
                var entity = dict[key];

                var oldFrequen = entity.Frequen;
                try
                {
                    dictFrequenNodeList[oldFrequen].Remove(entity);
                }
                catch { }

                var newFrequen = oldFrequen + 1;
                if (!dictFrequenNodeList.ContainsKey(newFrequen))
                {
                    dictFrequenNodeList.Add(newFrequen, new LinkedList<Node>());
                }

                newNode.Frequen = newFrequen;
                dictFrequenNodeList[newFrequen].AddLast(newNode);
                dict[key] = newNode;

                if (dictFrequenNodeList.ContainsKey(_minFreq) && dictFrequenNodeList[_minFreq].Count == 0)
                {
                    _minFreq = newFrequen;
                }
                return;
            }

            if (_capacity <= dict.Count)
            {
                var deleteNodeList = dictFrequenNodeList[_minFreq];
                var deleteFirstNode = deleteNodeList.First;
                deleteNodeList.RemoveFirst();
                try
                {
                    dict.Remove(deleteFirstNode.Value.Key);
                }
                catch { }
            }

            dictFrequenNodeList[0].AddLast(newNode);
            dict.Add(key, newNode);
            _minFreq = 0;
        }

        public void Remove(string key)
        {
            if (dict.TryGetValue(key, out Node node))
            {
                try
                {
                    dictFrequenNodeList[node.Frequen].Remove(node);
                }
                catch { }
                dict.Remove(key);
            }
        }

        class Node
        {
            public string Key;
            public object Value;
            public int Frequen;
            public Node(string key, object value, int frequen = 0)
            {
                this.Key = key;
                this.Value = value;
                this.Frequen = frequen;
            }
        }
    }
}
