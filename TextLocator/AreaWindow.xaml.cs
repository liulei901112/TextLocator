using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TextLocator.Core;
using TextLocator.Entity;
using TextLocator.Message;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// AreaWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AreaWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 正常区域列表区域信息列表
        /// </summary>
        private List<AreaInfo> _normalAreaInfos = new List<AreaInfo>();
        /// <summary>
        /// 删除区域信息列表
        /// </summary>
        private List<AreaInfo> _deleteAreaInfos = new List<AreaInfo>();

        public AreaWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 加载区域信息列表
            LoadAreaInfoList();
        }

        /// <summary>
        /// 加载区域信息列表
        /// <param name="areaInfos">区域信息列表</param>
        /// </summary>
        private void LoadAreaInfoList(List<AreaInfo> areaInfos = null)
        {
            // 外部传入区域信息列表为空
            if (areaInfos == null)
            {
                // 重新获取新的列表
                areaInfos = AreaUtil.GetAreaInfoList();
            }

            if (areaInfos != null)
            {
                this.AreaInfoList.Children.Clear();
                foreach (AreaInfo areaInfo in areaInfos)
                {
                    AreaInfoItem item = new AreaInfoItem(areaInfo);
                    // 编辑按钮
                    item.EditButton.Tag = areaInfo;
                    item.EditButton.Click += EditButton_Click;
                    // 删除按钮
                    item.DeleteButton.Tag = areaInfo;
                    item.DeleteButton.Click += DelButton_Click;
                    // 选中或取消选中事件
                    item.AreaIsEnable.Tag = areaInfo;
                    item.AreaIsEnable.Checked += AreaIsEnable_Checked;
                    item.AreaIsEnable.Unchecked += AreaIsEnable_Unchecked;
                    this.AreaInfoList.Children.Add(item);
                }
            }
            _normalAreaInfos = areaInfos;
        }

        /// <summary>
        /// 区域是否取用未选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AreaIsEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            AreaInfo areaInfo = (AreaInfo)(sender as CheckBox).Tag;
            areaInfo.IsEnable = false;

            // 修改本地列表缓存
            if (_normalAreaInfos!= null)
            {
                for(int i = 0; i < _normalAreaInfos.Count; i++)
                {
                    AreaInfo info = _normalAreaInfos[i];
                    if (info.AreaId.Equals(areaInfo.AreaId))
                    {
                        info = areaInfo;
                        _normalAreaInfos[i] = info;
                    }
                }
            }
        }

        /// <summary>
        /// 区域是否启用选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AreaIsEnable_Checked(object sender, RoutedEventArgs e)
        {
            AreaInfo areaInfo = (AreaInfo)(sender as CheckBox).Tag;
            areaInfo.IsEnable = true;

            // 修改本地列表缓存
            if (_normalAreaInfos != null)
            {
                for (int i = 0; i < _normalAreaInfos.Count; i++)
                {
                    AreaInfo info = _normalAreaInfos[i];
                    if (info.AreaId.Equals(areaInfo.AreaId))
                    {
                        info = areaInfo;
                        _normalAreaInfos[i] = info;
                    }
                }
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            AreaInfo areaInfo = (AreaInfo)(sender as Button).Tag;
            if (areaInfo != null)
            {
                // AreaUtil.DeleteAreaInfo(areaInfo);
                for( int i = 0; i < _normalAreaInfos.Count; i++)
                {
                    AreaInfo normalAreaInfo = _normalAreaInfos[i];
                    if (normalAreaInfo.AreaId == areaInfo.AreaId)
                    {
                        _normalAreaInfos.RemoveAt(i);
                        _deleteAreaInfos.Add(normalAreaInfo);
                        break;
                    }
                }

                // 重新加载区域信息列表（刷新）
                LoadAreaInfoList(_normalAreaInfos);
            }
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            AreaInfo areaInfo = (AreaInfo)(sender as Button).Tag;
            CacheUtil.Put("AreaInfos", _normalAreaInfos);
            ShowAreaEditDialog(areaInfo);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CacheUtil.Put("AreaInfos", _normalAreaInfos);
            ShowAreaEditDialog();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 保存正常的区域信息列表
            if (_normalAreaInfos != null)
            {
                int enableCount = 0;
                foreach (AreaInfo info in _normalAreaInfos)
                {
                    if (info.IsEnable) enableCount++;
                }

                if (enableCount < 1)
                {
                    MessageCore.ShowWarning("至少保留一个启用的搜索区");
                    return;
                }

                foreach (AreaInfo areaInfo in _normalAreaInfos)
                {
                    AreaUtil.SaveAreaInfo(areaInfo);
                }
            }
            // 保存删除的区域信息列表
            if (_deleteAreaInfos.Count > 0)
            {
                foreach (AreaInfo areaInfo in _deleteAreaInfos)
                {
                    areaInfo.AreaName = null;
                    areaInfo.AreaFolders = null;
                    AreaUtil.DeleteAreaInfo(areaInfo);


                    // 删除区域索引目录
                    try
                    {
                        string areaIndexDir = Path.Combine(AppConst.APP_INDEX_DIR, areaInfo.AreaId);
                        if (Directory.Exists(areaIndexDir))
                        {
                            Directory.Delete(areaIndexDir, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("删除区域【" + areaInfo.AreaId + "】索引目录失败：" + ex.Message, ex);
                    }
                }
            }
            this.DialogResult = true;
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        /// <summary>
        /// 显示区域编辑
        /// </summary>
        /// <param name="areaInfo"></param>
        private void ShowAreaEditDialog(AreaInfo areaInfo = null)
        {
            AreaEditWindow editDialog = new AreaEditWindow(areaInfo);
            editDialog.Topmost = true;
            editDialog.Owner = this;
            editDialog.ShowDialog();

            if (editDialog.DialogResult == true)
            {
                _normalAreaInfos = CacheUtil.Get<List<AreaInfo>>("AreaInfos");
                // 重新加载区域信息列表（刷新）
                LoadAreaInfoList(_normalAreaInfos);
            }
        }
    }
}
