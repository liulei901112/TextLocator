using log4net;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using TextLocator.Core;
using TextLocator.Message;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// HelpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HelpWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 单例
        /// </summary>
        private static HelpWindow _instance;

        public HelpWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 创建系统参数设置窗体实例
        /// </summary>
        /// <returns></returns>
        public static HelpWindow CreateInstance()
        {
            return _instance ?? (_instance = new HelpWindow());
        }

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
    }
}
