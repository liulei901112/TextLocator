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
    /// HotkeyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HotkeyWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 单例
        /// </summary>
        private static HotkeyWindow _instance;

        // 集合
        private ObservableCollection<HotKeyModel> _hotKeyList = new ObservableCollection<HotKeyModel>();
        public ObservableCollection<HotKeyModel> HotKeyList { get => _hotKeyList; set => _hotKeyList = value; }

        public HotkeyWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 创建系统参数设置窗体实例
        /// </summary>
        /// <returns></returns>
        public static HotkeyWindow CreateInstance()
        {
            return _instance ?? (_instance = new HotkeyWindow());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化快捷键
            InitHotKey();   
        }

        /// <summary>
        /// 初始化快捷键
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void InitHotKey()
        {
            var list = HotKeySettingManager.Instance.LoadDefaultHotKey();
            list.ToList().ForEach(x => HotKeyList.Add(x));
        }

        #region 保存并关闭
        /// <summary>
        /// 保存并关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveClose_Click(object sender, RoutedEventArgs e)
        {
            if (!HotKeySettingManager.Instance.RegisterGlobalHotKey(HotKeyList))
            {
                return;
            }
            foreach(HotKeyModel hotKey in HotKeyList)
            {
                log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(hotKey));
                AppUtil.WriteValue("HotKey", hotKey.Name, String.Format("{0}_{1}_{2}_{3}_{4}", hotKey.IsUsable, hotKey.IsSelectCtrl, hotKey.IsSelectAlt, hotKey.IsSelectShift, hotKey.SelectKey));
            }
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
    }
}
