using log4net;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TextLocator.Core;
using TextLocator.Entity;
using TextLocator.Enums;
using TextLocator.Message;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// AreaEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AreaEditWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 搜索区文件夹
        /// </summary>
        private List<string> _areaFolders = new List<string>();
        /// <summary>
        /// 搜索区文件类型
        /// </summary>
        private List<FileType> _areaFileTypes = new List<FileType>();
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
                    AreaId = "Area" + DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                    AreaFileTypes = FileTypeUtil.GetFileTypesNotAll()
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
            // 初始化加载文件类型
            LoadFileType();

            // 加载区域信息
            LoadAreaInfo();
        }

        /// <summary>
        /// 加载区域信息
        /// </summary>
        private void LoadAreaInfo()
        {
            // 区域名称
            this.AreaName.Text = _areaInfo.AreaName;
            // 区域文件夹
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
            // 区域文件夹类型
            foreach (UIElement element in this.AreaFileTypes.Children)
            {
                CheckBox checkbox = element as CheckBox;
                if (_areaInfo.AreaFileTypes.Contains((FileType)checkbox.Tag))
                {
                    checkbox.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// 初始化加载文件类型
        /// </summary>
        private void LoadFileType()
        {
            this.AreaFileTypes.Children.Clear();
            // 遍历文件类型枚举
            foreach (FileType fileType in FileTypeUtil.GetFileTypesNotAll())
            {
                // 构造UI元素
                CheckBox checkbox = new CheckBox()
                {
                    Name = "FileType_" + (int)fileType,
                    Margin = new Thickness(10,5,10,5),
                    Height = 20,
                    Tag = fileType,
                    Content = fileType.ToString() + "（" + fileType.GetDescription() + "）"
                };
                checkbox.Checked += FileTypeStatusChange;
                checkbox.Unchecked += FileTypeStatusChange;
                this.AreaFileTypes.Children.Add(checkbox);
            }
        }

        /// <summary>
        /// 文件类型选中状态切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileTypeStatusChange(object sender, RoutedEventArgs e)
        {
            List<FileType> fileTypes = new List<FileType>();
            foreach(UIElement element in this.AreaFileTypes.Children)
            {
                CheckBox checkBox = element as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    fileTypes.Add((Enums.FileType)System.Enum.Parse(typeof(Enums.FileType), checkBox.Tag.ToString()));
                }
            }
            _areaFileTypes = fileTypes;
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            // 当前删除文件夹路径
            string deleteFolderPath = (sender as Button).Tag + "";
            // UI层删除
            for (int i = 0; i < this.AreaFolders.Items.Count; i++)
            {
                if ((this.AreaFolders.Items[i] as FolderInfoItem).FolderPath.Text.Equals(deleteFolderPath))
                {
                    this.AreaFolders.Items.RemoveAt(i);
                    break;
                }
            }
            // 缓存列表层删除
            for (int i = 0; i < _areaFolders.Count; i++)
            {
                if (_areaFolders[i].Equals(deleteFolderPath))
                {
                    _areaFolders.RemoveAt(i);
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
                if (IsExistsAreaFolder(folderPath))
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
            if (IsExistsAreaName(areaName))
            {
                MessageCore.ShowWarning("区域名称不能重复");
                return;
            }
            if (this.AreaFolders.Items.Count <= 0)
            {
                MessageCore.ShowWarning("至少需要一个文件夹");
                return;
            }
            if (_areaFileTypes.Count <= 0)
            {
                MessageCore.ShowWarning("至少需要一个支持的文件类型");
                return;
            }

            // 搜索区名称
            _areaInfo.AreaName = areaName;
            // 搜索区文件夹
            _areaInfo.AreaFolders = _areaFolders;
            // 搜索区文件类型
            _areaInfo.AreaFileTypes = _areaFileTypes;

            // 保存区域信息
            // AreaUtil.SaveAreaInfo(_areaInfo);
            List<AreaInfo> areaInfos = CacheUtil.Get<List<AreaInfo>>(AppConst.CacheKey.AREA_INFOS_KEY);
            if (areaInfos.Contains(_areaInfo))
            {
                for (int i = 0; i < areaInfos.Count; i++)
                {
                    AreaInfo areaInfo = areaInfos[i];
                    if (_areaInfo.AreaId == areaInfo.AreaId)
                    {
                        areaInfos[i] = _areaInfo;
                        break;
                    }
                }
            }
            else
            {
                areaInfos.Add(_areaInfo);
            }
            
            CacheUtil.Put(AppConst.CacheKey.AREA_INFOS_KEY, areaInfos);

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

        /// <summary>
        /// 是否存在区域名称（如果是编辑的话，不包含自己）
        /// </summary>
        /// <param name="areaName"></param>
        /// <returns></returns>
        private bool IsExistsAreaName(string areaName)
        {
            List<AreaInfo> areaInfos = CacheUtil.Get<List<AreaInfo>>(AppConst.CacheKey.AREA_INFOS_KEY);
            if (areaInfos != null)
            {
                foreach (AreaInfo areaInfo in areaInfos)
                {
                    // 跳过区域ID相同的
                    if (!_areaInfo.AreaId.Equals(areaInfo.AreaId))
                    {
                        // 存在相同的区域
                        if (areaInfo.AreaName.Equals(areaName)) { return true; }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 是否存在区域文件夹
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns></returns>
        public bool IsExistsAreaFolder(string folderPath)
        {
            List<AreaInfo> areaInfos = CacheUtil.Get<List<AreaInfo>>(AppConst.CacheKey.AREA_INFOS_KEY);
            if (areaInfos != null)
            {
                foreach (AreaInfo areaInfo in areaInfos)
                {
                    // 跳过区域ID相同的
                    if (!_areaInfo.AreaId.Equals(areaInfo.AreaId))
                    {
                        // 文件夹已经存在
                        if (areaInfo.AreaFolders.Contains(folderPath))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
