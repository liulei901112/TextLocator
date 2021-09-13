using System;

namespace TextLocator.Core
{
    /// <summary>
    /// 工作时间对象，用于获取任务执行消耗时间
    /// </summary>
    public class WorkTime
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime beginTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WorkTime()
        {
            beginTime = DateTime.Now;
        }

        /// <summary>
        /// 消耗时间
        /// </summary>
        /// <returns></returns>
        public double TotalSeconds
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
        public static WorkTime StartNew()
        {
            return new WorkTime(); ;
        }
    }
}
