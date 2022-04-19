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
    /// AreaWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AreaWindow : Window
    {
        /// <summary>
        /// 区域信息列表
        /// </summary>
        private List<AreaInfo> _areaInfos;

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
            _areaInfos = areaInfos;
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
            if (_areaInfos!= null)
            {
                for(int i = 0; i < _areaInfos.Count; i++)
                {
                    AreaInfo info = _areaInfos[i];
                    if (info.AreaId.Equals(areaInfo.AreaId))
                    {
                        info = areaInfo;
                        _areaInfos[i] = info;
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
            if (_areaInfos != null)
            {
                for (int i = 0; i < _areaInfos.Count; i++)
                {
                    AreaInfo info = _areaInfos[i];
                    if (info.AreaId.Equals(areaInfo.AreaId))
                    {
                        info = areaInfo;
                        _areaInfos[i] = info;
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
                for( int i = 0; i < _areaInfos.Count; i++)
                {
                    if (_areaInfos[i].AreaId == areaInfo.AreaId)
                    {
                        _areaInfos.RemoveAt(i);
                        break;
                    }
                }

                // 重新加载区域信息列表（刷新）
                LoadAreaInfoList(_areaInfos);
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
            CacheUtil.Put("AreaInfos", _areaInfos);
            ShowAreaEditDialog(areaInfo);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CacheUtil.Put("AreaInfos", _areaInfos);
            ShowAreaEditDialog();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 修改本地列表缓存
            if (_areaInfos != null)
            {
                int enableCount = 0;
                foreach (AreaInfo info in _areaInfos)
                {
                    if (info.IsEnable) enableCount++;
                }

                if (enableCount < 1)
                {
                    MessageCore.ShowWarning("至少保留一个启用的搜索区");
                    return;
                }

                foreach (AreaInfo areaInfo in _areaInfos)
                {
                    AreaUtil.SaveAreaInfo(areaInfo);
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
                _areaInfos = CacheUtil.Get<List<AreaInfo>>("AreaInfos");
                // 重新加载区域信息列表（刷新）
                LoadAreaInfoList(_areaInfos);
            }
        }
    }
}
