using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
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
                        // Application.Current.MainWindow.Show();
                        SettingWindow setting = new SettingWindow();
                        setting.Show();
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
        /// 关闭软件
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }
}
