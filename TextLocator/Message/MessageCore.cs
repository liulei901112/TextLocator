using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TextLocator.Core;

namespace TextLocator.Message
{
    /// <summary>
    /// 消息盒子
    /// </summary>
    public class MessageCore
    {
        /// <summary>
        /// Rubyer.Message参数containerIdentifier
        /// </summary>
        private const string MESSAGE_CONTAINER = "MessageContainers";
        /// <summary>
        /// Rubyer.MessageBoxR参数containerIdentifier
        /// </summary>
        private const string MESSAGE_BOX_CONTAINER = "MessageBoxContainers";

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="message"></param>
        public static void ShowWarning(string message)
        {            
            void TryShow()
            {
                Rubyer.Message.ShowWarning(MESSAGE_CONTAINER, message);
            }
            try
            {
                TryShow();
            }
            catch
            {
                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    TryShow();
                });
            }
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="message"></param>
        public static void ShowSuccess(string message)
        {
            void TryShow()
            {
                Rubyer.Message.ShowSuccess(MESSAGE_CONTAINER, message);
            }
            try
            {
                TryShow();
            }
            catch
            {
                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    TryShow();
                });
            }
        }

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="message"></param>
        public static void ShowError(string message)
        {
            void TryShow()
            {
                Rubyer.Message.ShowError(MESSAGE_CONTAINER, message);
            }
            try
            {
                TryShow();
            }
            catch
            {
                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    TryShow();
                });
            }
        }

        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="message"></param>
        public static void ShowInfo(string message)
        {
            void TryShow()
            {
                Rubyer.Message.ShowInfo(MESSAGE_CONTAINER, message);
            }
            try
            {
                TryShow();
            }
            catch
            {
                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    TryShow();
                });
            }
        }

        /// <summary>
        /// 确认提示
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        public static Task<MessageBoxResult> ShowMessageBox(string message, string title, MessageBoxButton button = MessageBoxButton.OKCancel)
        {
            return Rubyer.MessageBoxR.ConfirmInContainer(MESSAGE_BOX_CONTAINER, message, title, button);
        }
    }
}
