using System;

namespace TextLocator.Core
{
    /// <summary>
    /// 任务时间
    /// </summary>
    public class TaskTime
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime beginTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TaskTime()
        {
            beginTime = DateTime.Now;
        }

        /// <summary>
        /// 消耗时间
        /// </summary>
        /// <returns></returns>
        public double ConsumeTime
        {
            get
            {
                return (DateTime.Now - beginTime).TotalSeconds;
            }
        }

        /// <summary>
        /// 开始新任务
        /// </summary>
        /// <returns></returns>
        public static TaskTime StartNew()
        {
            return new TaskTime();
        }
    }
}
