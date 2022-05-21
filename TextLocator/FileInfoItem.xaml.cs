using log4net;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TextLocator.Core;
using TextLocator.Index;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// FileInfoItem.xaml 的交互逻辑
    /// </summary>
    public partial class FileInfoItem : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 文件信息显示条目
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        public FileInfoItem(Entity.FileInfo fileInfo)
        {
            InitializeComponent();

            this.Tag = fileInfo;

            try
            {
                Refresh(fileInfo);
            }
            catch {
                Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        Refresh(fileInfo);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                    }
                });
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        public void Refresh(Entity.FileInfo fileInfo)
        {
            // 根据文件类型显示图标
            this.FileTypeIcon.Source = FileUtil.GetFileIcon(fileInfo.FileType);
            // 文件大小
            this.FileSize.Text = FileUtil.GetFileSizeFriendly(fileInfo.FileSize);
            // 更新时间
            this.UpdateTime.Text = fileInfo.UpdateTime;

            string fileName = fileInfo.FileName;
            // 显示文件名称
            FileContentUtil.FillFlowDocument(this.FileName, fileName.Length > 55 ? fileName.Substring(0, 55) + "..." : fileName, (Brush)new BrushConverter().ConvertFromString("#1A0DAB"), true);
            if (fileInfo.SearchRegion == Enums.SearchRegion.文件名和内容 || fileInfo.SearchRegion == Enums.SearchRegion.仅文件名)
            {
                FileContentUtil.FlowDocumentHighlight(this.FileName, Colors.Red, fileInfo.Keywords);
            }

            string folderPath = fileInfo.FilePath.Substring(0, fileInfo.FilePath.LastIndexOf("\\"));
            // 文件路径
            this.FileFolder.Text = folderPath.Length > 70 ? folderPath.Substring(0, 70) + "..." : folderPath;

            // 获取摘要
            FileContentUtil.EmptyRichTextDocument(this.ContentBreviary);
            Task.Factory.StartNew(async () => {
                await Task.Delay((AppConst.MRESULT_LIST_PAGE_SIZE - 10) * fileInfo.Index);
                string breviary = IndexCore.GetContentBreviary(fileInfo);
                await Dispatcher.InvokeAsync(() =>
                {
                    FileContentUtil.FillFlowDocument(this.ContentBreviary, breviary, (Brush)new BrushConverter().ConvertFromString("#545454"));
                    if (fileInfo.SearchRegion == Enums.SearchRegion.文件名和内容 || fileInfo.SearchRegion == Enums.SearchRegion.仅文件内容)
                    {
                        FileContentUtil.FlowDocumentHighlight(this.ContentBreviary, Colors.Red, fileInfo.Keywords);
                    }
                });
            });

            // 词频统计明细
            Task.Factory.StartNew(async () => {
                await Task.Delay((AppConst.MRESULT_LIST_PAGE_SIZE - 15) * fileInfo.Index);
                LoadMatchCountDetails(fileInfo);
            });
        }

        /// <summary>
        /// 光标在文件类型图标边界移入事件（词频统计详情放在这里加载，主要是为了节省列表加载事件）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileTypeIcon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.FileTypeIcon.ToolTip == null)
            {
                LoadMatchCountDetails(this.Tag as Entity.FileInfo);
            }
        }

        /// <summary>
        /// 加载词频统计详情放在这里
        /// </summary>
        private void LoadMatchCountDetails(Entity.FileInfo fileInfo)
        {
            string matchCountDetails = IndexCore.GetMatchCountDetails(fileInfo);
            if (!string.IsNullOrWhiteSpace(matchCountDetails))
            {
                void Load()
                {
                    this.FileTypeIcon.ToolTip = matchCountDetails;
                    ToolTipService.SetShowDuration(this.FileTypeIcon, 600000);
                }
                try
                {
                    Load();
                }
                catch
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        Load();
                    });
                }
            }
        }
    }
}
