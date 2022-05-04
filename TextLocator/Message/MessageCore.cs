using System.Threading.Tasks;
using System.Windows;
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
        private const string CONFIRM_CONTAINER = "ConfirmContainers";

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="message"></param>
        public static void ShowWarning(string message)
        {
            Rubyer.Message.ShowWarning(MESSAGE_CONTAINER, message);
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="message"></param>
        public static void ShowSuccess(string message)
        {
            Rubyer.Message.ShowSuccess(MESSAGE_CONTAINER, message);
        }

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="message"></param>
        public static void ShowError(string message)
        {
            Rubyer.Message.ShowError(MESSAGE_CONTAINER, message);
        }

        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="message"></param>
        public static void ShowInfo(string message)
        {
            Rubyer.Message.ShowInfo(MESSAGE_CONTAINER, message);
        }

        /// <summary>
        /// 确认提示
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        public static Task<MessageBoxResult> Confirm(string message, string title, MessageBoxButton button = MessageBoxButton.OKCancel)
        {
            return Rubyer.MessageBoxR.ConfirmInContainer(CONFIRM_CONTAINER, message, title, button);
        }
    }
}
