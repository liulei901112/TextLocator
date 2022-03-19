using System.Windows.Controls;

namespace TextLocator
{
    /// <summary>
    /// FolderInfoItem.xaml 的交互逻辑
    /// </summary>
    public partial class FolderInfoItem : UserControl
    {
        public FolderInfoItem(string folderPath)
        {
            InitializeComponent();

            Refresh(folderPath);
        }

        private void Refresh(string folderPath)
        {
            this.FolderPath.Text = folderPath;
            this.ToolTip = folderPath;
        }
    }
}
