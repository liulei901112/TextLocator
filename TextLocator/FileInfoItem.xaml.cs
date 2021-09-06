using log4net;
using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
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


            try
            {
                Refresh(fileInfo);
            }
            catch {
                this.Dispatcher.InvokeAsync(() =>
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
        /// <param name="fileInfo"></param>
        public void Refresh(Entity.FileInfo fileInfo)
        {
            // 根据文件类型显示图标
            this.FileTypeIcon.Source = FileUtil.GetFileIcon(fileInfo.FileType);

            // 显示文件信息
            this.FileName.Text = fileInfo.FileName;
            this.FileFolder.Text = fileInfo.FilePath.Replace(fileInfo.FileName, "");
            long fileSize = fileInfo.FileSize;
            string fileSizeUnit = "b";
            if (fileSize > 1024)
            {
                fileSize = fileSize / 1024;
                fileSizeUnit = "KB";
            }
            if (fileSize > 1024)
            {
                fileSize = fileSize / 1024;
                fileSizeUnit = "MB";
            }
            if (fileSize > 1024)
            {
                fileSize = fileSize / 1024;
                fileSizeUnit = "GB";
            }
            this.FileSize.Text = fileSize + "" + fileSizeUnit;
            this.CreateTime.Text = fileInfo.CreateTime;

            this.FileContent.Document.Blocks.Clear();
            // Paragraph 类似于 html 的 P 标签
            Paragraph p = new Paragraph();
            // Run 是一个 Inline 的标签
            Run r = new Run(fileInfo.Breviary);
            p.Inlines.Add(r);
            this.FileContent.Document.Blocks.Add(p);

            // 关键词高亮
            if (fileInfo.Keywords.Count > 0)
            {
                RichTextBoxUtil.Highlighted(this.FileContent, Colors.Red, fileInfo.Keywords);
            }
        }
    }
}
