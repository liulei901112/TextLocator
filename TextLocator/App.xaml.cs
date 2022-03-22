using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TextLocator.Core;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.Service;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // 托盘图标
        private static TaskbarIcon _taskbar;
        public static TaskbarIcon Taskbar { get => _taskbar; set => _taskbar = value; }

        // 单实例
        private Mutex _mutex;

        public App()
        {
            // 初始化线程池大小
            InitThreadPoolSize();

            // 初始化配置
            InitAppConfig();

            // 初始化文件服务引擎
            InitFileInfoServiceEngine();
        }

        /// <summary>
        /// 重写OnStartup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // 互斥
            _mutex = new Mutex(true, "TextLocator", out bool isNewInstance);
            // 是否启动新实例
            if (!isNewInstance)
            {
                // 找到已经在运行的实例句柄(给出你的窗体标题名 “XXX影院”)
                IntPtr hWndPtr = FindWindow(null, "文本搜索定位器");

                // 还原窗口
                _ = IsIconic(hWndPtr) ? ShowWindow(hWndPtr, SW_RESTORE) : ShowWindow(hWndPtr, SW_SHOW);

                // 激活窗口
                SetForegroundWindow(hWndPtr);
                
                // 退出当前实例
                AppCore.Shutdown();
                return;
            }

            // 托盘图标
            _taskbar = (TaskbarIcon)FindResource("Taskbar");

            base.OnStartup(e);

            // UI线程未捕获异常处理事件
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            // Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            // 非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        #region 初始化
        /// <summary>
        /// 初始化线程池大小
        /// </summary>
        private void InitThreadPoolSize()
        {
            bool setMinThread = ThreadPool.SetMinThreads(AppConst.THREAD_POOL_MIN_SIZE, AppConst.THREAD_POOL_MIN_SIZE);
            log.Debug("修改线程池最小线程数量：" + AppConst.THREAD_POOL_MIN_SIZE + " => " + setMinThread);
            bool setMaxThread = ThreadPool.SetMaxThreads(AppConst.THREAD_POOL_MAX_SIZE, AppConst.THREAD_POOL_MAX_SIZE);
            log.Debug("修改线程池最大线程数量：" + AppConst.THREAD_POOL_MAX_SIZE + " => " + setMaxThread);

            // 保存线程池
            AppUtil.WriteValue("ThreadPool", "MinSize", AppConst.THREAD_POOL_MIN_SIZE + "");
            AppUtil.WriteValue("ThreadPool", "MaxSize", AppConst.THREAD_POOL_MAX_SIZE + "");
        }

        /// <summary>
        /// 初始化文件信息服务引擎
        /// </summary>
        private void InitFileInfoServiceEngine()
        {
            try
            {
                log.Debug("初始化文件引擎工厂");
                // Word服务
                FileInfoServiceFactory.Register(FileType.Word文档, new WordFileService());
                // Excel服务
                FileInfoServiceFactory.Register(FileType.Excel表格, new ExcelFileService());
                // PowerPoint服务
                FileInfoServiceFactory.Register(FileType.PPT文稿, new PowerPointFileService());
                // PDF服务
                FileInfoServiceFactory.Register(FileType.PDF文档, new PdfFileService());
                // HTML或XML服务
                FileInfoServiceFactory.Register(FileType.DOM文档, new DomFileService());
                // 常用图片服务
                FileInfoServiceFactory.Register(FileType.图片, new NoTextFileService());
                // 程序员服务
                FileInfoServiceFactory.Register(FileType.代码, new DevelopFileService());
                // 纯文本服务
                FileInfoServiceFactory.Register(FileType.纯文本, new TxtFileService());
            }
            catch (Exception ex)
            {
                log.Error("文件服务工厂初始化错误：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 初始化AppConfig
        /// </summary>
        private void InitAppConfig()
        {
            // 保存线程池
            AppUtil.WriteValue("AppConfig", "FileReadTimeout", AppConst.FILE_READ_TIMEOUT + "");
        }
        #endregion

        #region 异常处理
        /// <summary>
        /// 非UI线程未捕获异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            if (e.IsTerminating)
            {
                builder.Append("非UI线程发生致命错误。");
            }
            builder.Append("非UI线程异常：");
            if (e.ExceptionObject is Exception)
            {
                builder.Append((e.ExceptionObject as Exception).Message);
            }
            else
            {
                builder.Append(e.ExceptionObject);
            }
            log.Error(builder.ToString());
        }

        /// <summary>
        /// Task线程内未捕获异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            log.Error("Task线程内未处理异常：" + e.Exception.Message, e.Exception);
            e.SetObserved();
        }

        /// <summary>
        /// UI线程未捕获异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                log.Error("UI线程未捕获异常：" + e.Exception.Message, e.Exception);
                // 处理完后，我们需要将Handler=true表示已此异常已处理过
                e.Handled = true;
            }
            catch (Exception ex)
            {
                log.Fatal("程序出现严重错误：" + ex.Message, ex);
            }
        }
        #endregion

        #region Windows API
        //ShowWindow 参数
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        /// <summary>
        /// 是标志性的
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns></returns>
        [DllImport("USER32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool IsIconic(IntPtr hWnd);
        /// <summary>
        /// 在桌面窗口列表中寻找与指定条件相符的第一个窗口。
        /// </summary>
        /// <param name="lpClassName">指向指定窗口的类名。如果 lpClassName 是 NULL，所有类名匹配。</param>
        /// <param name="lpWindowName">指向指定窗口名称(窗口的标题）。如果 lpWindowName 是 NULL，所有windows命名匹配。</param>
        /// <returns>返回指定窗口句柄</returns>
        [DllImport("USER32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 将窗口还原,可从最小化还原
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 激活指定窗口
        /// </summary>
        /// <param name="hWnd">指定窗口句柄</param>
        /// <returns></returns>
        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion
    }
}
