using Rubyer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// AreaWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AreaWindow : Window
    {
        /// <summary>
        /// 索引区文件夹
        /// </summary>
        private List<string> _indexFolder = new List<string>();
        /// <summary>
        /// 排除区文件夹
        /// </summary>
        private List<string> _exclusionFolder = new List<string>();

        public AreaWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取搜索文件夹
            string folderPaths = AppUtil.ReadValue("AppConfig", "FolderPaths", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "," + Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            // 清空
            this.FolderList.Items.Clear();
            if (!string.IsNullOrEmpty(folderPaths))
            {                
                FolderInfoItem folder;
                foreach (string folderPath in folderPaths.Split(','))
                {
                    folder = new FolderInfoItem(folderPath);
                    folder.DeleteButton.Click += DeleteButton_Click;
                    folder.DeleteButton.Tag = folderPath;

                    this.FolderList.Items.Add(folder);

                    _indexFolder.Add(folderPath);
                }
            }

            // 获取排除文件夹
            string exclusionPaths = AppUtil.ReadValue("AppConfig", "ExclusionPaths", "");
            // 清空
            this.ExclusionList.Items.Clear();
            if(!string.IsNullOrEmpty(exclusionPaths))
            {
                FolderInfoItem exclusion;
                foreach (string folderPath in exclusionPaths.Split(','))
                {
                    exclusion = new FolderInfoItem(folderPath);
                    exclusion.FolderPath.Foreground = new SolidColorBrush(Colors.Gray);
                    exclusion.DeleteButton.Click += DeleteExclusionButton_Click;
                    exclusion.DeleteButton.Tag = folderPath;

                    this.ExclusionList.Items.Add(exclusion);

                    _exclusionFolder.Add(folderPath);
                }
            }
        }

        #region 搜索区文件夹
        /// <summary>
        /// 条目删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < this.FolderList.Items.Count; i++)
            {
                if ((this.FolderList.Items[i] as FolderInfoItem).FolderPath.Text.Equals((sender as Button).Tag))
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

                // 判断是否已选过
                if (_indexFolder.Contains(folderPath))
                {
                    Message.ShowWarning("MessageContainer", "选定目录已存在");
                    return;
                }

                // 加入列表
                FolderInfoItem folder = new FolderInfoItem(folderPath);
                folder.DeleteButton.Click += DeleteButton_Click;
                folder.DeleteButton.Tag = folderPath;

                this.FolderList.Items.Add(folder);

                _indexFolder.Add(folderPath);
            }
        }
        #endregion

        #region 排除文件夹
        /// <summary>
        /// 排除文件夹条目删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteExclusionButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.ExclusionList.Items.Count; i++)
            {
                if ((this.ExclusionList.Items[i] as FolderInfoItem).FolderPath.Text.Equals((sender as Button).Tag))
                {
                    this.ExclusionList.Items.RemoveAt(i);
                    break;
                }
            }
        }
        /// <summary>
        /// 添加排除文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddExclusionFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // 获得选定文件夹
                string folderPath = browserDialog.SelectedPath;

                // 判断是否已选过
                if (_exclusionFolder.Contains(folderPath))
                {
                    Message.ShowWarning("MessageContainer", "选定目录已存在");
                    return;
                }

                // 判断是否存在于搜索区
                if (_indexFolder.Contains(folderPath))
                {
                    Message.ShowWarning("MessageContainer", "不能排除搜索区目录");
                    return;
                }

                // 加入列表
                FolderInfoItem folder = new FolderInfoItem(folderPath);
                folder.FolderPath.Foreground = new SolidColorBrush(Colors.Gray);
                folder.DeleteButton.Click += DeleteExclusionButton_Click;
                folder.DeleteButton.Tag = folderPath;

                this.ExclusionList.Items.Add(folder);

                _exclusionFolder.Add(folderPath);
            }
        }
        #endregion

        #region 保存并关闭
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
            // 搜索区文件夹
            string folderPaths = "";
            foreach(FolderInfoItem item in this.FolderList.Items)
            {
                folderPaths += item.FolderPath.Text + ",";
            }
            folderPaths = folderPaths.Substring(0, folderPaths.Length - 1);
            // 保存到配置文件
            AppUtil.WriteValue("AppConfig", "FolderPaths", folderPaths);

            // 排除文件夹
            string exclusionPaths = "";
            foreach (FolderInfoItem item in this.ExclusionList.Items)
            {
                exclusionPaths += item.FolderPath.Text + ",";
            }
            if (!string.IsNullOrEmpty(exclusionPaths))
            {
                exclusionPaths = exclusionPaths.Substring(0, exclusionPaths.Length - 1);
            }
            // 保存到配置文件
            AppUtil.WriteValue("AppConfig", "ExclusionPaths", exclusionPaths);

            this.DialogResult = true;

            this.Close();
        }
        #endregion
    }
}
