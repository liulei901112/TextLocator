using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TextLocator.Core;

namespace TextLocator.ViewModel.Main
{
    /// <summary>
    /// MainWindow模型
    /// </summary>
    public class MainViewModel: NotificationObject
    {
        private MainModel model;

        public MainViewModel()
        {
            model = new MainModel()
            {
                PageIndex = 1,
                PageSize = AppConst.MRESULT_LIST_PAGE_SIZE,
                TotalCount = 0,

                PreviewPage = "0/0",
                PreviewSwitchVisibility = Visibility.Hidden
            };
        }
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex
        {
            get { return model.PageIndex; }
            set
            {
                model.PageIndex = value;
                RaisePropertyChanged("PageIndex");
            }
        }
        /// <summary>
        /// 分页条数
        /// </summary>
        public int PageSize
        {
            // 获取值时将私有字段传出；
            get { return model.PageSize; }
            // 赋值时将值传给私有字段
            set
            {
                model.PageSize= value;
                RaisePropertyChanged("PageSize");
            }
        }
        /// <summary>
        /// 结果总条数
        /// </summary>
        public int TotalCount
        {
            get { return model.TotalCount; }
            set
            {
                model.TotalCount = value;
                RaisePropertyChanged("TotalCount");
            }
        }

        /// <summary>
        /// 预览分页
        /// </summary>
        public string PreviewPage {
            get { return model.PreviewPage; }
            set
            {
                model.PreviewPage = value;
                RaisePropertyChanged("PreviewPage");
            }
        }

        /// <summary>
        /// 切换预览显示状态
        /// </summary>
        public Visibility PreviewSwitchVisibility
        {
            get { return model.PreviewSwitchVisibility; }
            set
            {
                model.PreviewSwitchVisibility = value;
                RaisePropertyChanged("PreviewSwitchVisibility");
            }
        }
    }
}
