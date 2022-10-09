using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TextLocator.Core;

namespace TextLocator.NotifyIcon
{
    public class NotifyIconViewModel
    {
        /// <summary>
        /// 激活窗口
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        // 显示
                        Application.Current.MainWindow.Show();
                        // 标准化
                        Application.Current.MainWindow.Activate();
                    }
                };
            }
        }

        /// <summary>
        /// 热键设置
        /// </summary>
        public ICommand ShowHotKeyWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        var win = HotkeyWindow.CreateInstance();
                        if (!win.IsVisible)
                        {
                            win.Topmost = true;
                            win.Owner = Application.Current.MainWindow;
                            win.ShowDialog();
                        }
                        else
                        {
                            win.Activate();
                        }
                    }
                };
            }
        }
        
        /// <summary>
        /// 系统设置
        /// </summary>
        public ICommand ShowSettingWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        var win = SettingWindow.CreateInstance();
                        if (!win.IsVisible)
                        {
                            win.Topmost = true;
                            win.Owner = Application.Current.MainWindow;
                            win.ShowDialog();
                        }
                        else
                        {
                            win.Activate();
                        }
                    }
                };
            }
        }
        

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Hide()
                };
            }
        }

        /// <summary>
        /// 帮助窗口
        /// </summary>
        public ICommand HelpWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        var win = HelpWindow.CreateInstance();
                        if (!win.IsVisible)
                        {
                            win.Topmost = true;
                            win.Owner = Application.Current.MainWindow;
                            win.ShowDialog();
                        }
                        else
                        {
                            win.Activate();
                        }
                    }
                };
            }
        }


        /// <summary>
        /// 关闭软件
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => AppCore.Shutdown() };
            }
        }
    }
}
