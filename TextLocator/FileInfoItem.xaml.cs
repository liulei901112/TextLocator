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

        public FileInfoItem(Entity.FileInfo fileInfo)
        {
            InitializeComponent();

            this.Tag = fileInfo;

            try
            {
                Refresh(fileInfo);
            }
            catch {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        Refresh(fileInfo);
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
        /// <param name="fileInfo"></param>
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
            RichTextBoxUtil.FillingData(this.FileName, fileName.Length > 55 ? fileName.Substring(0, 55) + "..." : fileName, (Brush)new BrushConverter().ConvertFromString("#1A0DAB"), true);
            RichTextBoxUtil.Highlighted(this.FileName, Colors.Red, fileInfo.Keywords);

            string filePath = fileInfo.FilePath.Replace(fileInfo.FileName, "");
            // 文件路径
            RichTextBoxUtil.FillingData(this.FileFolder, filePath.Length > 70 ? filePath.Substring(0, 70) + "..." : filePath, (Brush)new BrushConverter().ConvertFromString("#006621"));
            
            // 获取摘要
            Task.Factory.StartNew(() => {
                string breviary = IndexCore.GetContentBreviary(fileInfo);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RichTextBoxUtil.FillingData(this.ContentBreviary, breviary, (Brush)new BrushConverter().ConvertFromString("#545454"));
                    RichTextBoxUtil.Highlighted(this.ContentBreviary, Colors.Red, fileInfo.Keywords);
                }));
            });
        }
    }
}
