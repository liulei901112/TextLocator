using log4net;
using System;
using System.Threading;

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
        private readonly ManualResetEvent _event;
        /// <summary>
        /// 任务总数
        /// </summary>
        private readonly int _total;
        /// <summary>
        /// 剩余数量
        /// </summary>
        private volatile int _current;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="total">需要等待执行的线程总数</param>
        public MutipleThreadResetEvent(int total)
        {
            this._total = total;
            this._current = total;
            this._event = new ManualResetEvent(false);
        }

        /// <summary>
        /// 唤醒一个等待的线程
        /// </summary>
        public void SetOne()
        {
            // Interlocked 原子操作类 ,此处将计数器减1
            if (Interlocked.Decrement(ref _current) == 0)
            {
                //当所以等待线程执行完毕时，唤醒等待的线程
                _event.Set();
            }
        }

        /// <summary>
        /// 等待所以线程执行完毕
        /// </summary>
        public void WaitAll()
        {
            _event.WaitOne();
        }

        /// <summary>
        /// 释放对象占用的空间
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)_event).Dispose();
        }
    }
}
