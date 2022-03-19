using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using System;
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

        private static TaskbarIcon _taskbar;

        public static TaskbarIcon Taskbar { get => _taskbar; set => _taskbar = value; }

        System.Threading.Mutex mutex;

        public App()
        {
            // 初始化线程池大小
            InitThreadPoolSize();

            // 初始化文件信息服务引擎
            InitFileInfoServiceEngine();
        }

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

        protected override void OnStartup(StartupEventArgs e)
        {
            bool ret;
            mutex = new Mutex(true, "TextLocator", out ret);
            if (!ret)
            {
                MessageBox.Show("程序已经在运行");
                log.Warn("程序已经在运行");
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
    }
}
