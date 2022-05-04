using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TextLocator.Util;

namespace TextLocator.Core
{
    /// <summary>
    /// AppCore
    /// </summary>
    public class AppCore
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 退出应用
        /// </summary>
        public static void Shutdown()
        {
            log.Debug("退出当前应用程序进程");
            Environment.Exit(Environment.ExitCode); //Application.Current.Shutdown(-1);            
        }

        /// <summary>
        /// 重启应用
        /// </summary>
        public static void Restart()
        {
            log.Debug("重启当前应用程序");

            // 启动新进程
            System.Reflection.Assembly.GetEntryAssembly();
            string processPath = Directory.GetCurrentDirectory();
            string processName = Process.GetCurrentProcess().ProcessName;
            Process.Start(processPath + "/" + processName);

            Shutdown();
        }

        /// <summary>
        /// 设置线程池大小
        /// </summary>
        public static void SetThreadPoolSize(bool optimalPerformance = true)
        {
            if (optimalPerformance)
            {
                bool setMinThread = ThreadPool.SetMinThreads(AppConst.THREAD_POOL_WORKER_MIN_SIZE, AppConst.THREAD_POOL_IO_MIN_SIZE);
                log.Debug(string.Format("设置线程池最小工作线程数：{0}，最小IO线程数：{1}，结果：{2}", AppConst.THREAD_POOL_WORKER_MIN_SIZE, AppConst.THREAD_POOL_IO_MIN_SIZE, setMinThread));
                bool setMaxThread = ThreadPool.SetMaxThreads(AppConst.THREAD_POOL_WORKER_MAX_SIZE, AppConst.THREAD_POOL_IO_MAX_SIZE);
                log.Debug(string.Format("设置线程池最大工作线程数：{0}，最大IO线程数：{1}，结果：{2}", AppConst.THREAD_POOL_WORKER_MAX_SIZE, AppConst.THREAD_POOL_IO_MAX_SIZE, setMaxThread));
                // 保存线程池
                AppUtil.WriteValue("ThreadPool", "WorkerMinSize", AppConst.THREAD_POOL_WORKER_MIN_SIZE + "");
                AppUtil.WriteValue("ThreadPool", "WorkerMaxSize", AppConst.THREAD_POOL_WORKER_MAX_SIZE + "");
                AppUtil.WriteValue("ThreadPool", "IOMinSize", AppConst.THREAD_POOL_IO_MIN_SIZE + "");
                AppUtil.WriteValue("ThreadPool", "IOMaxSize", AppConst.THREAD_POOL_IO_MAX_SIZE + "");
            }
            else
            {
                int wordMinSize = AppConst.THREAD_POOL_WORKER_MIN_SIZE / 2;
                int wordMaxSize = AppConst.THREAD_POOL_WORKER_MAX_SIZE / 2;
                int ioMinSize = AppConst.THREAD_POOL_IO_MIN_SIZE / 2;
                int ioMaxSize = AppConst.THREAD_POOL_IO_MAX_SIZE / 2;
                bool setMinThread = ThreadPool.SetMinThreads(wordMinSize, ioMinSize);
                log.Debug(string.Format("临时设置线程池最小工作线程数：{0}，最小IO线程数：{1}，结果：{2}", wordMinSize, ioMinSize, setMinThread));
                bool setMaxThread = ThreadPool.SetMaxThreads(wordMaxSize, ioMaxSize);
                log.Debug(string.Format("临时设置线程池最大工作线程数：{0}，最大IO线程数：{1}，结果：{2}", wordMaxSize, ioMaxSize, setMaxThread));
            }
        }

        /// <summary>
        /// 垃圾回收
        /// </summary>
        public static void ManualGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
