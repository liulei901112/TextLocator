using log4net;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
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
        /// <param name="searchRegion">搜索域</param>
        public FileInfoItem(Entity.FileInfo fileInfo, Enums.SearchRegion searchRegion)
        {
            InitializeComponent();

            this.Tag = fileInfo;

            try
            {
                Refresh(fileInfo, searchRegion);
            }
            catch {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        Refresh(fileInfo, searchRegion);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                    }
                }));
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="searchRegion">搜索域</param>
        public void Refresh(Entity.FileInfo fileInfo, Enums.SearchRegion searchRegion)
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
            if (searchRegion == Enums.SearchRegion.文件名和内容 || searchRegion == Enums.SearchRegion.仅文件名)
            {
                FileContentUtil.FlowDocumentHighlight(this.FileName, Colors.Red, fileInfo.Keywords);
            }

            string folderPath = fileInfo.FilePath.Substring(0, fileInfo.FilePath.LastIndexOf("\\"));
            // 文件路径
            this.FileFolder.Text = folderPath.Length > 70 ? folderPath.Substring(0, 70) + "..." : folderPath;

            // 获取摘要
            FileContentUtil.EmptyRichTextDocument(this.ContentBreviary);
            Task.Factory.StartNew(() => {
                string breviary = IndexCore.GetContentBreviary(fileInfo);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    FileContentUtil.FillFlowDocument(this.ContentBreviary, breviary, (Brush)new BrushConverter().ConvertFromString("#545454"));
                    if (searchRegion == Enums.SearchRegion.文件名和内容 || searchRegion == Enums.SearchRegion.仅文件内容)
                    {
                        FileContentUtil.FlowDocumentHighlight(this.ContentBreviary, Colors.Red, fileInfo.Keywords);
                    }
                }));
            });

            // 词频统计
            Task.Factory.StartNew(() => {
                string matchCountDetails = IndexCore.GetMatchCountDetails(fileInfo);
                this.Dispatcher.BeginInvoke(new Action(() => {
                    if (!string.IsNullOrWhiteSpace(matchCountDetails))
                    {
                        // 关键词匹配次数
                        this.FileTypeIcon.ToolTip = matchCountDetails;
                    }
                }));
            });
        }
    }
}
