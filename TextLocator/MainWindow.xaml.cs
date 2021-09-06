using log4net;
using Rubyer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TextLocator.Consts;
using TextLocator.Core;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.Index;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// 索引构建中
        /// </summary>
        private static volatile bool build = false;

        /// <summary>
        /// 索引文件夹列表
        /// </summary>
        private List<string> _IndexFolders = new List<string>();

        public MainWindow()
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
            // 初始化配置文件信息
            InitializeAppConfig();

            // 初始化文件类型过滤器列表
            InitializeFileTypeFilters();

            // 清理事件
            CleanSearchResult();

            // 检查索引是否存在
            CheckIndexExist();
        }

        #region 初始化

        /// <summary>
        /// 初始化文件类型过滤器列表
        /// </summary>
        private void InitializeFileTypeFilters()
        {
            // 文件类型筛选下拉框数据初始化
            this.FileTypeFilter.Items.Clear();
            this.FileTypeFilter.Items.Add("全部");
            // 获取文件类型枚举，遍历并加入下拉列表
            foreach (string fileTypeName in FileTypeUtil.GetFileTypes())
            {
                this.FileTypeFilter.Items.Add(fileTypeName);
            }
            // 默认选中全部
            this.FileTypeFilter.SelectedIndex = 0;
        }

        /// <summary>
        /// 初始化配置文件信息
        /// </summary>
        private void InitializeAppConfig()
        {
            // 初始化显示被索引的文件夹列表
            _IndexFolders.Clear();
            string customFolders = AppUtil.ReadIni("AppConfig", "FolderPaths", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "," + Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            if (!string.IsNullOrEmpty(customFolders))
            {
                string[] customFolderArray = customFolders.Split(',');
                foreach (string folderPath in customFolderArray)
                {
                    _IndexFolders.Add(folderPath);
                }
            }
            string foldersText = "";
            foreach (string folder in _IndexFolders)
            {
                foldersText += folder + ", ";
            }
            this.FolderPaths.Text = foldersText.Substring(0, foldersText.Length - 2);
            this.FolderPaths.ToolTip = this.FolderPaths.Text;


            // 初始化支持文件后缀列表
            this.FileExtensions.Text = AppConst.FILE_EXTENSIONS;
            this.FileExtensions.ToolTip = this.FileExtensions.Text;
        }

        #endregion

        #region 核心方法
        /// <summary>
        /// 检查索引是否存在
        /// </summary>
        /// <returns></returns>
        private bool CheckIndexExist()
        {
            bool exists = Directory.Exists(AppConst.APP_INDEX_DIR);
            if (!exists)
            {
                Message.ShowWarning("MessageContainer", "首次使用该软件，需先设置需要索引的文件夹。并点击右侧重建按钮进行初始化");
            }
            return exists;
        }

        /// <summary>
        /// 构建索引
        /// </summary>
        /// <param name="rebuild">重建，默认是优化</param>
        private void BuildIndex(bool rebuild = false)
        {
            Task.Factory.StartNew(() =>
            {
                DateTime beginMark = DateTime.Now;
                foreach (string s in _IndexFolders)
                {
                    // 获取文件信息列表
                    List<string> filePaths = FileUtil.GetAllFiles(s);
                    // 创建索引方法
                    LuceneIndexCore.CreateIndex(filePaths, rebuild, ShowStatus);
                }

                // 显示状态
                ShowStatus("索引执行结束，共用时：" + (DateTime.Now - beginMark).TotalSeconds + "秒");

                // 构建结束
                build = false;
            });            
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        /// <param name="text"></param>
        private void ShowStatus(string text)
        {
            this.Dispatcher.BeginInvoke(new Action(() => {
                this.WorkStatus.Text = text;
            }));
        }

        /// <summary>
        /// 检索关键字
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="fileTypeFilter">文件类型过滤器</param>
        private void Search(List<string> keywords, string fileTypeFilter)
        {
            string text = "";
            foreach(string kw in keywords) {
                text += kw + ",";
            }
            text = text.Substring(0, text.Length - 1);
            log.Debug("关键词：（" + text + "）, 文件类型：" + fileTypeFilter);
            // 开始时间标记
            DateTime beginMark = DateTime.Now;
            // 仅文件名
            bool onlyThsFileName = (bool)this.OnlyTheFileName.IsChecked;
            bool matchTheWords = (bool)this.MatchTheWords.IsChecked;

            // 清空搜索结果列表
            this.SearchResultList.Items.Clear();
            int num = 100;
            Lucene.Net.Index.IndexReader reader = null;
            Lucene.Net.Search.IndexSearcher searcher = null;
            try
            {
                if (!CheckIndexExist())
                {
                    return;
                }
                reader = Lucene.Net.Index.IndexReader.Open(AppConst.INDEX_DIRECTORY, false);
                searcher = new Lucene.Net.Search.IndexSearcher(reader);

                List<string> fields = new List<string>() { "FileName" };

                // 创建查询
                Lucene.Net.Analysis.PerFieldAnalyzerWrapper wrapper = new Lucene.Net.Analysis.PerFieldAnalyzerWrapper(AppConst.INDEX_ANALYZER);
                wrapper.AddAnalyzer("FileName", AppConst.INDEX_ANALYZER);

                // 仅文件名未被选中时
                if (!onlyThsFileName)
                {
                    wrapper.AddAnalyzer("FilePath", AppConst.INDEX_ANALYZER);
                    wrapper.AddAnalyzer("Content", AppConst.INDEX_ANALYZER);

                    fields.Add("FilePath");
                    fields.Add("Content");
                }

                // 匹配全词未被选中
                if (!matchTheWords)
                {

                }

                Lucene.Net.QueryParsers.QueryParser parser = 
                    new Lucene.Net.QueryParsers.MultiFieldQueryParser(
                        Lucene.Net.Util.Version.LUCENE_30, 
                        fields.ToArray(),
                        wrapper);
                Lucene.Net.Search.BooleanQuery Bquery = new Lucene.Net.Search.BooleanQuery();
                for (int i = 0; i < keywords.Count; i++)
                {
                    Lucene.Net.Search.Query query = parser.Parse(keywords[i]);
                    Bquery.Add(query, Lucene.Net.Search.Occur.MUST);
                }

                // 文件类型筛选
                if (!string.IsNullOrWhiteSpace(fileTypeFilter))
                {
                    Bquery.Add(new Lucene.Net.Search.TermQuery(new Lucene.Net.Index.Term("FileType", fileTypeFilter)), Lucene.Net.Search.Occur.MUST);
                }

                Lucene.Net.Search.TopScoreDocCollector collector = Lucene.Net.Search.TopScoreDocCollector.Create(num, true);
                searcher.Search(Bquery, collector);
                // 以后就可以对获取到的collector数据进行操作
                var hits = collector.TopDocs().ScoreDocs;                
                // 计算检索结果数量
                int resultNum = 0; 
                for (int i = 0; i < hits.Count(); i++)
                {
                    var hit = hits[i];
                    Lucene.Net.Documents.Document doc = searcher.Doc(hit.Doc);
                    Lucene.Net.Documents.Field fileTypeField = doc.GetField("FileType");
                    Lucene.Net.Documents.Field fileNameField = doc.GetField("FileName");
                    Lucene.Net.Documents.Field filePathField = doc.GetField("FilePath");
                    Lucene.Net.Documents.Field contentField = doc.GetField("Content");
                    Lucene.Net.Documents.Field breviaryField = doc.GetField("Breviary");
                    Lucene.Net.Documents.Field fileSizeField = doc.GetField("FileSize");
                    Lucene.Net.Documents.Field createTimeField = doc.GetField("CreateTime");

                    // 判断本地是否存在该文件，存在则在检索结果栏里显示出来
                    if (!System.IO.File.Exists(filePathField.StringValue))
                    {
                        // 该文件的在索引里的文档号,Doc是该文档进入索引时Lucene的编号，默认按照顺序编的
                        int docId = hit.Doc;
                        // 删除该索引
                        reader.DeleteDocument(docId);
                        reader.Commit();
                        continue;
                    }

                    // 文件信息
                    FileInfo fi = new FileInfo(filePathField.StringValue);


                    log.Debug(fileNameField.StringValue + " => " + filePathField.StringValue + " ， " + fileSizeField.StringValue + " , " + createTimeField.StringValue);

                    Entity.FileInfo fileInfo = new Entity.FileInfo()
                    {
                        FileType = (FileType)System.Enum.Parse(typeof(FileType), fileTypeField.StringValue),

                        FileName = fileNameField.StringValue,
                        FilePath = filePathField.StringValue,
                        Breviary = breviaryField.StringValue,

                        FileSize = long.Parse(fileSizeField.StringValue),

                        CreateTime = createTimeField.StringValue,
                        Keywords = keywords
                    };

                    FileInfoItem infoItem = new FileInfoItem(fileInfo);
                    // 增加点击事件
                    infoItem.MouseDown += InfoItem_MouseDown;
                    infoItem.Tag = fileInfo;
                    this.SearchResultList.Items.Add(infoItem);

                    resultNum++;
                }

                string message = "检索完成！共检索到" + resultNum + "个符合条件的结果（只显示前" + num + "条）。耗时：" + (DateTime.Now - beginMark).TotalSeconds + "秒";

                Message.ShowSuccess("MessageContainer", message);

                ShowStatus(message);
            }
            finally
            {
                if (searcher != null)
                    searcher.Dispose();

                if (reader != null)
                    reader.Dispose();
            }
        }
        #endregion

        #region 功能按钮事件
        /// <summary>
        /// 搜索结果列表点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 手动GC
            GC.Collect();
            GC.WaitForPendingFinalizers();

            FileInfoItem infoItem = sender as FileInfoItem;
            Entity.FileInfo fileInfo = infoItem.Tag as Entity.FileInfo;

            // 根据文件类型显示图标
            this.PreviewFileTypeIcon.Source = FileUtil.GetFileTypeIcon(fileInfo.FileType);
            this.PreviewFileName.Text = fileInfo.FileName;
            this.PreviewFileContent.Document.Blocks.Clear();

            // 绑定打开文件和打开路径的Tag
            this.OpenFile.Tag = fileInfo.FilePath;
            this.OpenFolder.Tag = fileInfo.FilePath.Replace(fileInfo.FileName, "");

            // 获取扩展名
            string fileExt = Path.GetExtension(fileInfo.FilePath).Replace(".", "");

            // 图片文件
            if ("png,jpg,gif".Contains(fileExt))
            {
                this.PreviewFileContent.Visibility = Visibility.Hidden;
                this.PreviewImage.Visibility = Visibility.Visible;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = new MemoryStream(File.ReadAllBytes(fileInfo.FilePath));
                bi.EndInit();
                bi.Freeze();
                
                this.PreviewImage.Source = bi;

            }
            else
            {
                this.PreviewImage.Visibility = Visibility.Hidden;
                this.PreviewFileContent.Visibility = Visibility.Visible;                
                // 文件内容预览
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    // 文件内容
                    string content = FileInfoServiceFactory.GetFileInfoService(fileInfo.FileType).GetFileContent(fileInfo.FilePath);

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Paragraph 类似于 html 的 P 标签
                        Paragraph p = new Paragraph();
                        // Run 是一个 Inline 的标签
                        Run r = new Run(content);
                        p.Inlines.Add(r);

                        this.PreviewFileContent.Document.Blocks.Add(p);

                        // 关键词高亮
                        RichTextBoxUtil.Highlighted(this.PreviewFileContent, Colors.Red, fileInfo.Keywords);
                    }));
                });
            }
        }


        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取搜索关键词列表
            List<string> keywords = GetTextKeywords();

            if (keywords == null || keywords.Count <= 0)
            {
                Message.ShowWarning("MessageContainer", "请输入搜索关键词");
                return;
            }
            BeforeSearch();
        }

        /// <summary>
        /// 回车搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchText_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchText.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                BeforeSearch();
            }
        }

        /// <summary>
        /// 文件类型过滤选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileTypeFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BeforeSearch();
        }

        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            CleanSearchResult();
        }

        /// <summary>
        /// 优化按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (build)
            {
                Message.ShowWarning("MessageContainer", "索引构建中，请稍等。");
                return;
            }

            BuildIndex(false);
        }

        /// <summary>
        /// 重建按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RebuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (build)
            {
                Message.ShowWarning("MessageContainer", "索引构建中，请稍等。");
                return;
            }
            build = true;

            BuildIndex(true);
        }

        /// <summary>
        /// 搜索区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderPaths_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FolderWindow folderDialog = new FolderWindow();
            folderDialog.ShowDialog();
            if (folderDialog.DialogResult == true)
            {
                InitializeAppConfig();
            }
        }

        #endregion

        #region 右侧预览区域
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFile_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.OpenFile.Tag != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(this.OpenFile.Tag + "");
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolder_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.OpenFolder.Tag != null)
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", @"" + this.OpenFolder.Tag);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
        }
        #endregion

        #region 其他私有封装
        /// <summary>
        /// 获取文本关键词
        /// </summary>
        /// <returns></returns>
        private List<string> GetTextKeywords()
        {
            string text = this.SearchText.Text;
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            List<string> keywords = new List<string>();
            if (text.IndexOf(" ") != -1)
            {
                string[] texts = text.Split(' ');
                foreach (string txt in texts)
                {
                    keywords.Add(txt);
                }
            }
            else
            {
                keywords.Add(text);
            }
            return keywords;
        }

        /// <summary>
        /// 清理查询结果
        /// </summary>
        private void CleanSearchResult()
        {
            this.FileTypeFilter.SelectedIndex = 0;
            this.SearchText.Text = "";
            this.SearchResultList.Items.Clear();
            this.PreviewFileName.Text = "";
            this.PreviewFileContent.Document.Blocks.Clear();
            this.WorkStatus.Text = "就绪";
            this.OnlyTheFileName.IsChecked = false;
            this.MatchTheWords.IsChecked = false;
            this.OpenFile.Tag = null;
            this.OpenFolder.Tag = null;
        }

        /// <summary>
        /// 搜索前
        /// </summary>
        private void BeforeSearch()
        {
            object filter = this.FileTypeFilter.SelectedValue;
            if (filter == null || filter.Equals("全部"))
            {
                filter = null;
            }

            // 获取搜索关键词列表
            List<string> keywords = GetTextKeywords();

            if (keywords == null || keywords.Count <= 0)
            {
                return;
            }
            if (build)
            {
                Message.ShowWarning("MessageContainer", "索引构建中，请稍等。");
                return;
            }

            Search(keywords, filter == null ? null : filter + "");
        }
        #endregion
    }
}
