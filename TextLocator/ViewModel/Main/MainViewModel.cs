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
                PreviewSwitchVisibility = Visibility.Hidden,

                WorkStatus = "就绪",
                WorkProgress = 100,
                ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal,
                ProgressValue = 100
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

        /// <summary>
        /// 工作状态
        /// </summary>
        public string WorkStatus
        {
            get { return model.WorkStatus; }
            set
            {
                model.WorkStatus = value;
                RaisePropertyChanged("WorkStatus");
            }
        }
        /// <summary>
        /// 工作进度
        /// </summary>
        public double WorkProgress
        {
            get { return model.WorkProgress; }
            set
            {
                model.WorkProgress = value;
                RaisePropertyChanged("WorkProgress");
            }
        }

        /// <summary>
        /// 任务栏图标状态
        /// </summary>
        public System.Windows.Shell.TaskbarItemProgressState ProgressState
        {
            get { return model.ProgressState; }
            set
            {
                model.ProgressState = value;
                RaisePropertyChanged("ProgressState");
            }
        }
        /// <summary>
        /// 任务栏进度
        /// </summary>
        public double ProgressValue
        {
            get { return model.ProgressValue; }
            set
            {
                model.ProgressValue = value;
                RaisePropertyChanged("ProgressState");
            }
        }
    }
}
