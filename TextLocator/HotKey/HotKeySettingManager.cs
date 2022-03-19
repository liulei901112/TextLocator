using System;
using System.Collections.ObjectModel;
using TextLocator.Enums;
using TextLocator.Util;

namespace TextLocator.HotKey
{
    /// <summary>
    /// 快捷键设置管理器
    /// </summary>
    public class HotKeySettingManager
    {
        private static HotKeySettingManager m_Instance;
        /// <summary>
        /// 单例实例
        /// </summary>
        public static HotKeySettingManager Instance
        {
            get { return m_Instance ?? (m_Instance = new HotKeySettingManager()); }
        }

        /// <summary>
        /// 加载默认快捷键
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<HotKeyModel> LoadDefaultHotKey()
        {
            var hotKeyList = new ObservableCollection<HotKeyModel>();
            foreach(HotKeySetting keySetting in Enum.GetValues(typeof(HotKeySetting)))
            {
                // 读取配置
                string config = AppUtil.ReadValue("HotKey", keySetting.ToString(), "");
                HotKeyModel keyModel = null;
                if (string.IsNullOrEmpty(config))
                {
                    keyModel = new HotKeyModel
                    {
                        Name = keySetting.ToString(),
                        IsUsable = true,
                        IsSelectCtrl = true,
                        IsSelectAlt = true,
                        IsSelectShift = false,
                        SelectKey = (HotKey)Enum.Parse(typeof(HotKey), keySetting.GetDescription())
                    };
                }
                else
                {
                    var tmp = config.Split('_');
                    keyModel = new HotKeyModel
                    {
                        Name = keySetting.ToString(),
                        IsUsable = bool.Parse(tmp[0]),
                        IsSelectCtrl = bool.Parse(tmp[1]),
                        IsSelectAlt = bool.Parse(tmp[2]),
                        IsSelectShift = bool.Parse(tmp[3]),
                        SelectKey = (HotKey)Enum.Parse(typeof(HotKey), tmp[4])
                    };
                }
                hotKeyList.Add(keyModel);
            }
            return hotKeyList;
        }

        /// <summary>
        /// 通知注册系统快捷键委托
        /// </summary>
        /// <param name="hotKeyModelList"></param>
        public delegate bool RegisterGlobalHotKeyHandler(ObservableCollection<HotKeyModel> hotKeyModelList);
        public event RegisterGlobalHotKeyHandler RegisterGlobalHotKeyEvent;
        public bool RegisterGlobalHotKey(ObservableCollection<HotKeyModel> hotKeyModelList)
        {
            if (RegisterGlobalHotKeyEvent != null)
            {
                return RegisterGlobalHotKeyEvent(hotKeyModelList);
            }
            return false;
        }

    }
}
