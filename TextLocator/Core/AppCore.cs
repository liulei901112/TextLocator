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
        public static void SetThreadPoolSize()
        {
            bool setMinThread = ThreadPool.SetMinThreads(AppConst.THREAD_POOL_MIN_SIZE, AppConst.THREAD_POOL_MIN_SIZE);
            log.Debug("修改线程池最小线程数量：" + AppConst.THREAD_POOL_MIN_SIZE + " => " + setMinThread);
            bool setMaxThread = ThreadPool.SetMaxThreads(AppConst.THREAD_POOL_MAX_SIZE, AppConst.THREAD_POOL_MAX_SIZE);
            log.Debug("修改线程池最大线程数量：" + AppConst.THREAD_POOL_MAX_SIZE + " => " + setMaxThread);

            // 保存线程池
            AppUtil.WriteValue("ThreadPool", "MinSize", AppConst.THREAD_POOL_MIN_SIZE + "");
            AppUtil.WriteValue("ThreadPool", "MaxSize", AppConst.THREAD_POOL_MAX_SIZE + "");
        }
    }
}
