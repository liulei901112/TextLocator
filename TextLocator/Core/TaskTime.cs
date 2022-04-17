using System;

namespace TextLocator.Core
{
    /// <summary>
    /// 任务时间
    /// </summary>
    public class TaskTime
    {
        /// <summary>
        /// 文件大小单位
        /// </summary>
        private static readonly string[] suffixes = new string[] { " 秒", " 分", " 时" };

        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime beginTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        private TaskTime()
        {
            beginTime = DateTime.Now;
        }

        /// <summary>
        /// 消耗时间（友好显示）
        /// </summary>
        public string ConsumeTime
        {
            get {
                double time = (DateTime.Now - beginTime).TotalMilliseconds;
                if (time > 1000)
                {
                    if (time / 1000 < 60)
                    {
                        return time / 1000 + " 秒";
                    }
                    else if (time / 1000 / 60 < 60)
                    {
                        return time / 1000 / 60 + " 分";
                    }
                    else if (time / 1000 / 60 / 60 < 24)
                    {
                        return time / 1000 / 60 / 60 + " 时";
                    }
                }
                return time + " 毫秒";
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
