using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }
    }
}
