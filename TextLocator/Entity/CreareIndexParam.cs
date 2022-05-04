using System.Collections.Generic;
using TextLocator.Core;

namespace TextLocator.Entity
{
    /// <summary>
    /// 创建索引参数
    /// </summary>
    public class CreareIndexParam
    {

        /// <summary>
        /// 状态回调委托
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="percent">进度条比例，默认最大值</param>
        public delegate void CallbackStatus(string msg, double percent = AppConst.MAX_PERCENT);

        /// <summary>
        /// 区域信息
        /// </summary>
        public string AreaId { get; set; }
        /// <summary>
        /// 当前区域索引
        /// </summary>
        public int AreaIndex { get; set; }
        /// <summary>
        /// 区域总数
        /// </summary>
        public int AreasCount { get; set; }
        /// <summary>
        /// 更新列表
        /// </summary>
        public List<string> UpdateFilePaths { get; set; }
        /// <summary>
        /// 删除列表
        /// </summary>
        public List<string> DeleteFilePaths { get; set; }
        /// <summary>
        /// 是否重建
        /// </summary>
        public bool IsRebuild { get; set; }
        /// <summary>
        /// 回调函数
        /// </summary>
        public CallbackStatus Callback { get; set; }
    }
}
