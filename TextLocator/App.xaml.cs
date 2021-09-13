using log4net;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.Service;

namespace TextLocator
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public App()
        {
            try
            {
                // Word服务
                FileInfoServiceFactory.Register(FileType.Word文档, new WordFileService());
                // Excel服务
                FileInfoServiceFactory.Register(FileType.Excel表格, new ExcelFileService());
                // PowerPoint服务
                FileInfoServiceFactory.Register(FileType.PPT演示文稿, new PowerPointFileService());
                // PDF服务
                FileInfoServiceFactory.Register(FileType.PDF文档, new PdfFileService());
                // HTML或XML服务
                FileInfoServiceFactory.Register(FileType.HTML和XML, new XmlFileService());
                // 常用图片服务
                FileInfoServiceFactory.Register(FileType.常用图片, new NoTextFileService());
                // 程序员服务
                FileInfoServiceFactory.Register(FileType.代码文件, new DevelopFileService());
                // 纯文本服务
                FileInfoServiceFactory.Register(FileType.文本文件, new TxtFileService());
            }
            catch (Exception ex)
            {
                log.Error("文件服务工厂初始化错误：" + ex.Message, ex);
            }


            bool setMinThread = ThreadPool.SetMinThreads(12, 12);
            log.Debug("修改线程池最小线程数量：" + setMinThread);
            bool setMaxThread = ThreadPool.SetMaxThreads(24, 24);
            log.Debug("修改线程池最大线程数量：" + setMaxThread);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
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
