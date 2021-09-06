using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Core
{
    /// <summary>
    /// AppCore
    /// </summary>
    public class AppCore
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region App退出或重启
        /// <summary>
        /// 退出应用
        /// </summary>
        public static void ExitProcess()
        {
            log.Debug("退出当前应用程序进程");
            Environment.Exit(Environment.ExitCode); //Application.Current.Shutdown(-1);            
        }

        /// <summary>
        /// 重启应用
        /// </summary>
        public static void RestartProcess()
        {
            log.Debug("重启当前应用程序");

            // 启动新进程
            System.Reflection.Assembly.GetEntryAssembly();
            String processPath = Directory.GetCurrentDirectory();
            String processName = Process.GetCurrentProcess().ProcessName;
            Process.Start(processPath + "/" + processName);

            ExitProcess();
        }
        #endregion
    }
}
