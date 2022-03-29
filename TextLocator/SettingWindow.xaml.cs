using log4net;
using Newtonsoft.Json;
using Rubyer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TextLocator.Core;
using TextLocator.HotKey;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 单例
        /// </summary>
        private static SettingWindow _instance;

        public SettingWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 创建系统参数设置窗体实例
        /// </summary>
        /// <returns></returns>
        public static SettingWindow CreateInstance()
        {
            return _instance ?? (_instance = new SettingWindow());
        }

        /// <summary>
        /// 加载完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 加载配置
            LoadConfig();
        }

        /// <summary>
        /// 加载配置信息
        /// </summary>
        private void LoadConfig()
        {
            // 线程池
            this.MinThreads.Text = AppConst.THREAD_POOL_MIN_SIZE + "";
            this.MaxThreads.Text = AppConst.THREAD_POOL_MAX_SIZE + "";

            // 每页显示条数
            this.ResultListPageSize.Text = AppConst.MRESULT_LIST_PAGE_SIZE + "";

            // 文件读取超时时间
            this.FileReadTimeout.Text = AppConst.FILE_READ_TIMEOUT + "";

            // 缓存池容量
            this.CachePoolCapacity.Text = AppConst.CACHE_POOL_CAPACITY + "";
        }

        #region 保存并关闭
        /// <summary>
        /// 保存并关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveClose_Click(object sender, RoutedEventArgs e)
        {
            // 线程池
            string minThreadsText = this.MinThreads.Text;
            string maxThreadsText = this.MaxThreads.Text;

            // 每页显示条数
            string ResultListPageSizeText = this.ResultListPageSize.Text;

            // 文件读取超时时间
            string fileReadTimeoutText = this.FileReadTimeout.Text;

            // 缓存池容量
            string cachePoolCapacityText = this.CachePoolCapacity.Text;

            // 转换，验证
            int minThreads = 0;
            try
            {
                minThreads = int.Parse(minThreadsText);
            }
            catch
            {
                Message.ShowWarning("MessageContainer", "最小线程数错误");
                return;
            }
            int maxThreads = 0;
            try
            {
                maxThreads = int.Parse(maxThreadsText);
            }
            catch
            {
                Message.ShowWarning("MessageContainer", "最大线程数错误");
                return;
            }
            if (minThreads > maxThreads)
            {
                Message.ShowWarning("MessageContainer", "最小线程数大于最大线程数");
                return;
            }
            if (maxThreads > 128)
            {
                var result = await MessageBoxR.ConfirmInContainer("DialogContaioner", "线程数不是越大越好，你确定吗？", "提示");
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            int ResultListPageSize = 0;
            try
            {
                ResultListPageSize = int.Parse(ResultListPageSizeText);
            }
            catch
            {
                Message.ShowWarning("MessageContainer", "分页条数错误");
                return;
            }
            if (ResultListPageSize < 50 || ResultListPageSize > 300)
            {
                Message.ShowWarning("MessageContainer", "建议设置在50 - 300范围内");
                return;
            }

            int fileReadTimeout = 0;
            try
            {
                fileReadTimeout = int.Parse(fileReadTimeoutText);
            }
            catch
            {
                Message.ShowWarning("MessageContainer", "文件读取超时时间错误");
                return;
            }
            if (fileReadTimeout < 5 * 60 || fileReadTimeout > 15 * 60)
            {
                Message.ShowWarning("MessageContainer", "建议设置在5 - 15分钟范围内");
                return;
            }

            int cachePoolCapacity = 0;
            try
            {
                cachePoolCapacity = int.Parse(cachePoolCapacityText);
            } catch
            {
                Message.ShowWarning("MessageContainer", "缓存池容量设置错误");
                return;
            }
            if (cachePoolCapacity < 50000 || cachePoolCapacity > 500000)
            {
                Message.ShowWarning("MessageContainer", "建议设置在5-50w范围内");
                return;
            }

            // 刷新、保存
            AppConst.THREAD_POOL_MIN_SIZE = minThreads;
            AppConst.THREAD_POOL_MAX_SIZE = maxThreads;
            AppCore.SetThreadPoolSize();

            AppConst.MRESULT_LIST_PAGE_SIZE = ResultListPageSize;
            AppUtil.WriteValue("AppConfig", "ResultListPageSize", AppConst.MRESULT_LIST_PAGE_SIZE + "");
            log.Debug("修改结果列表分页条数：" + AppConst.MRESULT_LIST_PAGE_SIZE);


            AppConst.FILE_READ_TIMEOUT = fileReadTimeout;
            AppUtil.WriteValue("AppConfig", "FileReadTimeout", AppConst.FILE_READ_TIMEOUT + "");
            log.Debug("修改文件读取超时时间：" + AppConst.FILE_READ_TIMEOUT);

            AppConst.CACHE_POOL_CAPACITY = cachePoolCapacity;
            AppUtil.WriteValue("AppConfig", "CachePoolCapacity", AppConst.CACHE_POOL_CAPACITY + "");
            log.Debug("修改缓存池容量：" + AppConst.CACHE_POOL_CAPACITY);

            this.Close();
        }
        #endregion

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }

        /// <summary>
        /// 数字文本框预览输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Number_TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.-]+").IsMatch(e.Text);
        }
    }
}
