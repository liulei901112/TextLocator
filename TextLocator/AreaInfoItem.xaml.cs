using log4net;
using System;
using System.Windows.Controls;
using TextLocator.Entity;

namespace TextLocator
{
    /// <summary>
    /// AreaInfoItem.xaml 的交互逻辑
    /// </summary>
    public partial class AreaInfoItem : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AreaInfoItem(AreaInfo areaInfo)
        {
            InitializeComponent();

            try
            {
                Refresh(areaInfo);
            }
            catch
            {
                Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        Refresh(areaInfo);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                    }
                });
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="areaInfo">区域信息</param>
        public void Refresh(AreaInfo areaInfo)
        {
            // 是否启用
            this.AreaIsEnable.IsChecked = areaInfo.IsEnable;
            // 区域名称
            this.AreaName.Text = areaInfo.AreaName;
            this.AreaIsEnable.ToolTip = this.AreaName.Text;
            // 区域文件夹
            this.AreaFolders.Children.Clear();
            if (areaInfo.AreaFolders != null)
            {
                foreach (string path in areaInfo.AreaFolders)
                {
                    TextBlock text = new TextBlock() { Text = path };
                    this.AreaFolders.Children.Add(text);
                }
            }
            // 区域文件类型
            this.AreaFileTypes.Text =  string.Join("，", areaInfo.AreaFileTypes.ToArray());
        }
    }
}
