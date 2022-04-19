using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TextLocator.Entity;
using TextLocator.Message;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// AreaEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AreaEditWindow : Window
    {
        /// <summary>
        /// 索引区文件夹
        /// </summary>
        private List<string> _areaFolders = new List<string>();
        /// <summary>
        /// 区域信息
        /// </summary>
        private AreaInfo _areaInfo;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="areaInfo">区域信息对象</param>
        public AreaEditWindow(AreaInfo areaInfo = null)
        {
            InitializeComponent();
            _areaInfo = areaInfo;

            if (_areaInfo == null)
            {
                _areaInfo = new AreaInfo()
                {
                    AreaId = "Area" + DateTime.Now.ToString("yyyyMMddHHmmssffff")
                };
            }
        }

        /// <summary>
        /// 加载完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.AreaName.Text = _areaInfo.AreaName;
            this.AreaFolders.Items.Clear();
            if (_areaInfo.AreaFolders != null)
            {
                FolderInfoItem folder;
                foreach (string folderPath in _areaInfo.AreaFolders)
                {
                    _areaFolders.Add(folderPath);

                    folder = new FolderInfoItem(folderPath);
                    folder.DelButton.Click += DelButton_Click; ;
                    folder.DelButton.Tag = folderPath;
                    this.AreaFolders.Items.Add(folder);
                }
            }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.AreaFolders.Items.Count; i++)
            {
                if ((this.AreaFolders.Items[i] as FolderInfoItem).FolderPath.Text.Equals((sender as Button).Tag))
                {
                    this.AreaFolders.Items.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = browserDialog.SelectedPath;

                // 判断是否已选过
                if (_areaFolders.Contains(folderPath))
                {
                    MessageCore.ShowWarning("选定目录已存在");
                    return;
                }

                // 判断文件夹是否存在于其他区域中
                if (AreaUtil.GetAreaFolderList().Contains(folderPath))
                {
                    MessageCore.ShowWarning("该文件夹已存在于其他区域");
                    return;
                }

                // 加入列表
                FolderInfoItem folder = new FolderInfoItem(folderPath);
                folder.DelButton.Click += DelButton_Click;
                folder.DelButton.Tag = folderPath;

                this.AreaFolders.Items.Add(folder);

                _areaFolders.Add(folderPath);
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string areaName = this.AreaName.Text.Trim();

            if (string.IsNullOrEmpty(areaName))
            {
                MessageCore.ShowWarning("区域名称为空");
                return;
            }
            if (this.AreaFolders.Items.Count <= 0)
            {
                MessageCore.ShowWarning("至少需要一个文件夹");
                return;
            }
            if (AreaUtil.GetAreaNameListRuleOut(_areaInfo).Contains(areaName))
            {
                MessageCore.ShowWarning("区域名称不能重复");
                return;
            }

            // 搜索区名称
            _areaInfo.AreaName = areaName;
            // 搜索区文件夹
            _areaInfo.AreaFolders = _areaFolders;

            // 保存区域信息
            // AreaUtil.SaveAreaInfo(_areaInfo);
            List<AreaInfo> areaInfos = CacheUtil.Get<List<AreaInfo>>("AreaInfos");
            if (areaInfos.Contains(_areaInfo))
            {
                for (int i = 0; i < areaInfos.Count; i++)
                {
                    AreaInfo areaInfo = areaInfos[i];
                    if (_areaInfo.AreaId == areaInfo.AreaId)
                    {
                        areaInfos[i] = _areaInfo;
                    }
                    break;
                }
            }
            else
            {
                areaInfos.Add(_areaInfo);
            }
            
            CacheUtil.Put("AreaInfos", areaInfos);

            // 返回
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
