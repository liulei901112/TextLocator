using Rubyer;
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
using System.Windows.Shapes;
using TextLocator.Consts;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// FolderWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FolderWindow : Window
    {
        public FolderWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取已有搜索去
            string folderPaths = AppUtil.ReadIni("AppConfig", "FolderPaths", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "," + Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            if (!string.IsNullOrEmpty(folderPaths))
            {
                // 清空
                this.FolderList.Items.Clear();

                foreach (string folderPath in folderPaths.Split(','))
                {
                    FolderInfoItem folder = new FolderInfoItem(folderPath);
                    folder.DeleteButton.Click += DeleteButton_Click;
                    folder.DeleteButton.Tag = folderPath;

                    this.FolderList.Items.Add(folder);
                }
            }
        }

        /// <summary>
        /// 条目删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < this.FolderList.Items.Count; i++)
            {
                if ((this.FolderList.Items[i] as FolderInfoItem).FolderPath.Text == (sender as Button).Tag)
                {
                    this.FolderList.Items.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = browserDialog.SelectedPath;

                FolderInfoItem folder = new FolderInfoItem(folderPath);
                folder.DeleteButton.Click += DeleteButton_Click;
                folder.DeleteButton.Tag = folderPath;

                this.FolderList.Items.Add(folder);
            }
        }

        /// <summary>
        /// 保存并关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveClose_Click(object sender, RoutedEventArgs e)
        {
            if (this.FolderList.Items.Count <= 0)
            {
                Message.ShowWarning("至少保留一个被搜索文件夹哦");
                return;
            }
            string folderPaths = "";
            foreach(FolderInfoItem item in this.FolderList.Items)
            {
                folderPaths += item.FolderPath.Text + ",";
            }
            folderPaths = folderPaths.Substring(0, folderPaths.Length - 1);

            // 保存到配置文件
            AppUtil.WriteIni("AppConfig", "FolderPaths", folderPaths);

            this.DialogResult = true;

            this.Close();
        }
    }
}
