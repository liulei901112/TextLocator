using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TextLocator.Enums;

namespace TextLocator.Entity
{
    /// <summary>
    /// 搜索参数
    /// </summary>
    [Serializable]
    public class SearchParam
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public List<string> Keywords { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public FileType FileType { get; set; }
        /// <summary>
        /// 排序规则
        /// </summary>
        public SortType SortType { get; set; }
        /// <summary>
        /// 精确检索
        /// </summary>
        public bool IsPreciseRetrieval { get; set; }
        /// <summary>
        /// 匹配全词
        /// </summary>
        public bool IsMatchWords { get; set; }
        /// <summary>
        /// 搜索域
        /// </summary>
        public SearchRegion SearchRegion { get; set; }
        /// <summary>
        /// 分页索引
        /// </summary>
        private int pageIndex = 1;
        public int PageIndex { get => pageIndex; set => pageIndex = value; }
        /// <summary>
        /// 分页大小
        /// </summary>
        private int pageSize = 100;
        public int PageSize { get => pageSize; set => pageSize = value; }

        /// <summary>
        /// 克隆一个新对象
        /// </summary>
        /// <returns></returns>
        public SearchParam Clone()
        {
            object obj = null;
            //将对象序列化成内存中的二进制流
            BinaryFormatter inputFormatter = new BinaryFormatter();
            MemoryStream inputStream;
            using (inputStream = new MemoryStream())
            {
                inputFormatter.Serialize(inputStream, this);
            }
            //将二进制流反序列化为对象
            using (MemoryStream outputStream = new MemoryStream(inputStream.ToArray()))
            {
                BinaryFormatter outputFormatter = new BinaryFormatter();
                obj = outputFormatter.Deserialize(outputStream);
            }
            return (SearchParam)obj;
        }
    }
}
