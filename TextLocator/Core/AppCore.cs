using log4net;
using System;
using System.Diagnostics;
using System.IO;

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
    }
}
