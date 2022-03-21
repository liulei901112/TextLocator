using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextLocator.Index
{
    /// <summary>
    /// 多线程重置事件
    /// </summary>
    public class MutipleThreadResetEvent : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 人工重置的事件
        /// </summary>
        private readonly ManualResetEvent done;
        /// <summary>
        /// 任务总数
        /// </summary>
        private readonly int total;
        /// <summary>
        /// 任务当前剩余数量
        /// </summary>
        private long current;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="total">需要等待执行的线程总数</param>
        public MutipleThreadResetEvent(int total)
        {
            this.total = total;
            this.current = total;
            this.done = new ManualResetEvent(false);
        }

        /// <summary>
        /// 唤醒一个等待的线程
        /// </summary>
        public void SetOne()
        {
            // Interlocked 原子操作类 ,此处将计数器减1
            if (Interlocked.Decrement(ref current) == 0)
            {
                //当所以等待线程执行完毕时，唤醒等待的线程
                done.Set();
            }
        }

        /// <summary>
        /// 等待所以线程执行完毕
        /// </summary>
        public void WaitAll(int millisecondsTimeout = 5 * 60 * 1000)
        {
            done.WaitOne(millisecondsTimeout);
        }

        /// <summary>
        /// 释放对象占用的空间
        /// </summary>
        public void Dispose()
        {
            done.Dispose();
        }
    }
}
