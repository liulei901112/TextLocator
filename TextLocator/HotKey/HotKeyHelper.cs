using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace TextLocator.HotKey
{
    /// <summary>
    /// 热键注册帮助
    /// </summary>
    public class HotKeyHelper
    {
        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private static Dictionary<HotKeySetting, int> _hotKeySettingDic = new Dictionary<HotKeySetting, int>();

        /// <summary>
        /// 注册全局快捷键
        /// </summary>
        /// <param name="hotKeyModelList">待注册快捷键项</param>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKeySettingsDic">快捷键注册项的唯一标识符字典</param>
        /// <returns>返回注册失败项的拼接字符串</returns>
        public static string RegisterGlobalHotKey(IEnumerable<HotKeyModel> hotKeyModelList, IntPtr hwnd, out Dictionary<HotKeySetting, int> hotKeySettingsDic)
        {
            string failList = string.Empty;
            foreach (var item in hotKeyModelList)
            {
                if (!RegisterHotKey(item, hwnd))
                {
                    string str = string.Empty;
                    if (item.IsSelectCtrl && !item.IsSelectShift && !item.IsSelectAlt)
                    {
                        str = ModifierKeys.Control.ToString();
                    }
                    else if (!item.IsSelectCtrl && item.IsSelectShift && !item.IsSelectAlt)
                    {
                        str = ModifierKeys.Shift.ToString();
                    }
                    else if (!item.IsSelectCtrl && !item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = ModifierKeys.Alt.ToString();
                    }
                    else if (item.IsSelectCtrl && item.IsSelectShift && !item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}", ModifierKeys.Control.ToString(), ModifierKeys.Shift);
                    }
                    else if (item.IsSelectCtrl && !item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}", ModifierKeys.Control.ToString(), ModifierKeys.Alt);
                    }
                    else if (!item.IsSelectCtrl && item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}", ModifierKeys.Shift.ToString(), ModifierKeys.Alt);
                    }
                    else if (item.IsSelectCtrl && item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}+{2}", ModifierKeys.Control.ToString(), ModifierKeys.Shift.ToString(), ModifierKeys.Alt);
                    }
                    if (string.IsNullOrEmpty(str))
                    {
                        str += item.SelectKey;
                    }
                    else
                    {
                        str += string.Format("+{0}", item.SelectKey);
                    }
                    str = string.Format("{0} ({1})\n\r", item.Name, str);
                    failList += str;
                }
            }
            hotKeySettingsDic = _hotKeySettingDic;
            return failList;
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hotKeyModel">热键待注册项</param>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>成功返回true，失败返回false</returns>
        private static bool RegisterHotKey(HotKeyModel hotKeyModel, IntPtr hWnd)
        {
            var fsModifierKey = new ModifierKeys();
            var hotKeySetting = (HotKeySetting)Enum.Parse(typeof(HotKeySetting), hotKeyModel.Name);

            if (!_hotKeySettingDic.ContainsKey(hotKeySetting))
            {
                // 全局原子不会在应用程序终止时自动删除。每次调用GlobalAddAtom函数，必须相应的调用GlobalDeleteAtom函数删除原子。
                if (HotKeyManager.GlobalFindAtom(hotKeySetting.ToString()) != 0)
                {
                    HotKeyManager.GlobalDeleteAtom(HotKeyManager.GlobalFindAtom(hotKeySetting.ToString()));
                }
                // 获取唯一标识符
                _hotKeySettingDic[hotKeySetting] = HotKeyManager.GlobalAddAtom(hotKeySetting.ToString());
            }
            else
            {
                // 注销旧的热键
                HotKeyManager.UnregisterHotKey(hWnd, _hotKeySettingDic[hotKeySetting]);
            }
            if (!hotKeyModel.IsUsable)
                return true;

            // 注册热键
            if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift;
            }
            else if (!hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift;
            }
            else if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Alt;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift | ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt;
            }

            return HotKeyManager.RegisterHotKey(hWnd, _hotKeySettingDic[hotKeySetting], fsModifierKey, (int)hotKeyModel.SelectKey);
        }

    }
}
