using System.Collections.Generic;

namespace TextLocator.Cache
{
    /// <summary>
    /// LRU（Least Recently Used）缓存机制
    /// 从数据集中，挑选最近最少使用的数据淘汰
    /// </summary>
    public class LRUCache
    {
        private Node head;
        private Node end;
        private int limit;
        private Dictionary<string, Node> dict;

        public LRUCache(int limit)
        {
            this.limit = limit;
            dict = new Dictionary<string, Node>();
        }

        public T Get<T>(string key)
        {
            if (!dict.TryGetValue(key, out Node node))
            {
                return default(T);
            }
            RefreshNode(node);
            try
            {
                return (T)node.Value;
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
            if (!dict.TryGetValue(key, out Node node))
            {
                if (dict.Count >= limit)
                {
                    string oldKey = RemoveNode(head);
                    dict.Remove(oldKey);
                }
                node = new Node(key, value);
                AddNode(node);
                dict.Add(key, node);
            }
            else
            {
                node.Value = value;
                RefreshNode(node);
            }
        }

        public void Remove(string key)
        {
            if (dict.TryGetValue(key, out Node node))
            {
                RemoveNode(node);
                dict.Remove(key);
            }
        }

        private void RefreshNode(Node node)
        {
            if (node == end)
                return;
            RemoveNode(node);
            AddNode(node);
        }

        private void AddNode(Node node)
        {
            if (end != null)
            {
                end.Next = node;
                node.Pre = end;
                node.Next = null;
            }
            end = node;
            if (head == null)
                head = node;
        }

        private string RemoveNode(Node node)
        {
            if (node == head && node == end)
            {
                head = null;
                end = null;
            }
            else if (node == end)
            {
                end = end.Pre;
                end.Next = null;
            }
            else if (node == head)
            {
                head = head.Next;
                head.Pre = null;
            }
            else
            {
                node.Pre.Next = node.Next;
                node.Next.Pre = node.Pre;
            }
            return node.Key;
        }

        class Node
        {
            public string Key;
            public object Value;
            public Node Pre;
            public Node Next;
            public Node(string key, object value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
    }
}
