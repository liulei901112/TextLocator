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
using TextLocator.Message;
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
            // 每页显示条数
            this.ResultListPageSize.Text = AppConst.MRESULT_LIST_PAGE_SIZE + "";

            // 文件读取超时时间
            this.FileReadTimeout.Text = AppConst.FILE_READ_TIMEOUT + "";

            // 缓存池容量
            this.CachePoolCapacity.Text = AppConst.CACHE_POOL_CAPACITY + "";

            // 启用索引更新任务
            this.EnableIndexUpdateTask.IsChecked = AppConst.ENABLE_INDEX_UPDATE_TASK;
            // 索引更新时间间隔
            this.IndexUpdateTaskInterval.Text = AppConst.INDEX_UPDATE_TASK_INTERVAL + "";

            // 启用预览内容摘要
            this.EnablePreviewSummary.IsChecked = AppConst.ENABLE_PREVIEW_SUMMARY;
        }

        #region 保存并关闭
        /// <summary>
        /// 保存并关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveClose_Click(object sender, RoutedEventArgs e)
        {
            // 缓存池容量
            string cachePoolCapacityText = this.CachePoolCapacity.Text;
            int cachePoolCapacity = 0;
            try
            {
                cachePoolCapacity = int.Parse(cachePoolCapacityText);
            }
            catch
            {
                MessageCore.ShowWarning("缓存池容量设置错误");
                return;
            }
            if (cachePoolCapacity < 50000 || cachePoolCapacity > 500000)
            {
                MessageCore.ShowWarning("建议设置在5-50W范围内");
                return;
            }

            // 每页显示条数
            string ResultListPageSizeText = this.ResultListPageSize.Text;
            int ResultListPageSize = 0;
            try
            {
                ResultListPageSize = int.Parse(ResultListPageSizeText);
            }
            catch
            {
                MessageCore.ShowWarning("分页条数错误");
                return;
            }
            if (ResultListPageSize < 50 || ResultListPageSize > 300)
            {
                MessageCore.ShowWarning("建议设置在50 - 300范围内");
                return;
            }

            // 文件读取超时时间
            string fileReadTimeoutText = this.FileReadTimeout.Text;
            int fileReadTimeout = 0;
            try
            {
                fileReadTimeout = int.Parse(fileReadTimeoutText);
            }
            catch
            {
                MessageCore.ShowWarning("文件读取超时时间错误");
                return;
            }
            if (fileReadTimeout < 5 * 60 || fileReadTimeout > 15 * 60)
            {
                MessageCore.ShowWarning("建议设置在5 - 15分钟范围内");
                return;
            }

            // 启用索引更新任务
            bool enableIndexUpdateTask = (bool)this.EnableIndexUpdateTask.IsChecked;
            if (enableIndexUpdateTask)
            {
                // 索引更新时间间隔
                string indexUpdateTaskIntervalText = this.IndexUpdateTaskInterval.Text;
                int indexUpdateTaskInterval = 0;
                try
                {
                    indexUpdateTaskInterval = int.Parse(indexUpdateTaskIntervalText);
                }
                catch
                {
                    MessageCore.ShowWarning("索引更新任务间隔时间错误");
                    return;
                }
                if (indexUpdateTaskInterval < 5 || indexUpdateTaskInterval > 30)
                {
                    MessageCore.ShowWarning("建议设置在5 - 30分钟范围内");
                    return;
                }

                AppConst.INDEX_UPDATE_TASK_INTERVAL = indexUpdateTaskInterval;
            }

            // 启用预览内容摘要
            bool enablePreviewSummary = (bool)this.EnablePreviewSummary.IsChecked;

            AppConst.CACHE_POOL_CAPACITY = cachePoolCapacity;
            AppUtil.WriteValue("AppConfig", "CachePoolCapacity", AppConst.CACHE_POOL_CAPACITY + "");
            log.Debug("修改缓存池容量：" + AppConst.CACHE_POOL_CAPACITY);

            AppConst.MRESULT_LIST_PAGE_SIZE = ResultListPageSize;
            AppUtil.WriteValue("AppConfig", "ResultListPageSize", AppConst.MRESULT_LIST_PAGE_SIZE + "");
            log.Debug("修改结果列表分页条数：" + AppConst.MRESULT_LIST_PAGE_SIZE);

            AppConst.FILE_READ_TIMEOUT = fileReadTimeout;
            AppUtil.WriteValue("AppConfig", "FileReadTimeout", AppConst.FILE_READ_TIMEOUT + "");
            log.Debug("修改文件读取超时时间：" + AppConst.FILE_READ_TIMEOUT);

            AppConst.ENABLE_INDEX_UPDATE_TASK = enableIndexUpdateTask;
            AppUtil.WriteValue("AppConfig", "EnableIndexUpdateTask", AppConst.ENABLE_INDEX_UPDATE_TASK + "");
            log.Debug("修改使用索引更新任务：" + AppConst.ENABLE_INDEX_UPDATE_TASK);

            if (enableIndexUpdateTask)
            {                
                AppUtil.WriteValue("AppConfig", "IndexUpdateTaskInterval", AppConst.INDEX_UPDATE_TASK_INTERVAL + "");
                log.Debug("修改索引更新任务间隔时间：" + AppConst.INDEX_UPDATE_TASK_INTERVAL);
            }

            AppConst.ENABLE_PREVIEW_SUMMARY = enablePreviewSummary;
            AppUtil.WriteValue("AppConfig", "EnableIndexUpdateTask", AppConst.ENABLE_PREVIEW_SUMMARY + "");

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
            _instance.Topmost = false;
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
