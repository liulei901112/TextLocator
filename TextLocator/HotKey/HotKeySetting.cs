using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.HotKey
{
    /// <summary>
    /// 快捷键设置项枚举, Description为默认键
    /// </summary>
    public enum HotKeySetting
    {
        /// <summary>
        /// 显示
        /// </summary>
        [Description("D")]
        显示 = 0,
        /// <summary>
        /// 隐藏
        /// </summary>
        [Description("H")]
        隐藏 = 1,
        /// <summary>
        /// 清空
        /// </summary>
        [Description("C")]
        清空 = 2,
        /// <summary>
        /// 退出
        /// </summary>
        [Description("E")]
        退出 = 3,
    }
}
