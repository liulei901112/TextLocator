using log4net;
using Newtonsoft.Json;
using Rubyer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        }

        #region 保存并关闭
        /// <summary>
        /// 保存并关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveClose_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
