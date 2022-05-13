using System.ComponentModel;

namespace TextLocator.ViewModel
{
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 发起通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
