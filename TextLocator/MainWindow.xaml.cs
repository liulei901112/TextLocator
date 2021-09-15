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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

            // 检查配置参数信息
            if (string.IsNullOrEmpty(AppUtil.ReadValue("AppConfig", "MaxCountLimit", "")))
            {
                AppUtil.WriteValue("AppConfig", "MaxCountLimit", AppConst.MAX_COUNT_LIMIT + "");
            }

            // 软件每次启动时执行索引更新逻辑？
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
            this.FileTypeNames.Children.Clear();

            // 获取文件类型枚举，遍历并加入下拉列表
            foreach (FileType fileType in Enum.GetValues(typeof(FileType)))
            {
                this.FileTypeFilter.Items.Add(fileType.ToString());

                // 标签
                Button ft = new Button()
                {
                    Content = fileType.ToString(),
                    Height = 25,
                    Margin = new Thickness(this.FileTypeNames.Children.Count == 0 ? 0 : 2, 0, 0, 0),
                    ToolTip = fileType.GetDescription(),
                    Background = Brushes.Gray
                };
                this.FileTypeNames.Children.Add(ft);
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
            // 读取被索引文件夹配置信息，如果配置信息为空：默认为我的文档和我的桌面
            string customFolders = AppUtil.ReadValue("AppConfig", "FolderPaths", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "," + Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            // 配置信息不为空
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
        }

        #endregion

        #region 核心方法
        /// <summary>
        /// 检查索引是否存在
        /// </summary>
        /// <returns></returns>
        private bool CheckIndexExist(bool showWarning = true)
        {
            bool exists = Directory.Exists(AppConst.APP_INDEX_DIR);
            if (!exists)
            {
                if (showWarning)
                {
                    Message.ShowWarning("MessageContainer", "首次使用该软件，需先设置需要索引的文件夹。并点击右侧重建按钮进行初始化");
                }
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
                var taskMark = TaskTime.StartNew();

                // 定义文件列表
                List<string> filePaths = new List<string>();
                foreach (string s in _IndexFolders)
                {
                    log.Debug("目录：" + s);
                    // 获取文件信息列表
                    FileUtil.GetAllFiles(s, filePaths);
                }

                // 创建索引方法
                IndexCore.CreateIndex(filePaths, rebuild, ShowStatus);

                // 索引拷贝前删除
                FileUtil.RemoveDirectory(AppConst.APP_INDEX_DIR);

                // 索引拷贝：索引创建结束后拷贝新索引覆盖旧的索引，并删除write.lock
                FileUtil.CopyDirectory(AppConst.APP_INDEX_BUILD_DIR, AppConst.APP_INDEX_DIR);

                string msg = "索引执行结束，共用时：" + taskMark.ConsumeTime + "秒";

                // 显示状态
                ShowStatus(msg);

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Message.ShowSuccess("MessageContainer", msg);
                }));

                // 构建结束
                build = false;
            });
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        /// <param name="text"></param>
        /// <param name="percent"></param>
        private void ShowStatus(string text, double percent = 100)
        {
            this.Dispatcher.BeginInvoke(new Action(() => {
                this.WorkStatus.Text = text;
                if (percent > 0)
                {
                    this.WorkProgress.Value = percent;
                }
            }));
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="fileType">文件类型</param>
        /// <param name="onlyFileName">仅文件名</param>
        /// <param name="matchWords">匹配全词</param>
        private void Search(List<string> keywords, string fileType, bool onlyFileName = false, bool matchWords = false)
        {
            if (!CheckIndexExist())
            {
                return;
            }

            Thread t = new Thread(() => {
                // 清空搜索结果列表
                this.Dispatcher.BeginInvoke(new Action(() => {
                    this.SearchResultList.Items.Clear();
                }));

                // 开始时间标记
                var taskMark = TaskTime.StartNew();

                Lucene.Net.Index.IndexReader reader = null;
                Lucene.Net.Search.IndexSearcher searcher = null;
                try
                {
                    reader = Lucene.Net.Index.IndexReader.Open(AppConst.INDEX_DIRECTORY, false);
                    searcher = new Lucene.Net.Search.IndexSearcher(reader);

                    List<string> fields = new List<string>() { "FileName" };

                    // 创建查询
                    Lucene.Net.Analysis.PerFieldAnalyzerWrapper wrapper = new Lucene.Net.Analysis.PerFieldAnalyzerWrapper(AppConst.INDEX_ANALYZER);
                    wrapper.AddAnalyzer("FileName", AppConst.INDEX_ANALYZER);

                    // 仅文件名未被选中时
                    if (!onlyFileName)
                    {
                        wrapper.AddAnalyzer("FilePath", AppConst.INDEX_ANALYZER);
                        wrapper.AddAnalyzer("Content", AppConst.INDEX_ANALYZER);

                        fields.Add("FilePath");
                        fields.Add("Content");
                    }

                    // 匹配全词未被选中
                    if (!matchWords)
                    {
                        List<string> segmentList = new List<string>();
                        for (int i = 0; i < keywords.Count; i++)
                        {
                            segmentList.AddRange(AppConst.INDEX_SEGMENTER.Cut(keywords[i]).ToList());
                        }
                        // 合并关键词
                        keywords = keywords.Union(segmentList).ToList();
                    }

                    string text = "";
                    foreach (string k in keywords)
                    {
                        text += k + ",";
                    }
                    text = text.Substring(0, text.Length - 1);
                    log.Debug("关键词：（" + text + "）, 文件类型：" + fileType);

                    Lucene.Net.QueryParsers.QueryParser parser =
                        new Lucene.Net.QueryParsers.MultiFieldQueryParser(
                            Lucene.Net.Util.Version.LUCENE_30,
                            fields.ToArray(),
                            wrapper);
                    Lucene.Net.Search.BooleanQuery Bquery = new Lucene.Net.Search.BooleanQuery();
                    for (int i = 0; i < keywords.Count; i++)
                    {
                        Lucene.Net.Search.Query query = parser.Parse(keywords[i]);
                        Bquery.Add(query, matchWords ? Lucene.Net.Search.Occur.MUST : Lucene.Net.Search.Occur.SHOULD);
                    }

                    // 文件类型筛选
                    if (!string.IsNullOrWhiteSpace(fileType))
                    {
                        Bquery.Add(new Lucene.Net.Search.TermQuery(new Lucene.Net.Index.Term("FileType", fileType)), Lucene.Net.Search.Occur.MUST);
                    }

                    Lucene.Net.Search.TopScoreDocCollector collector = Lucene.Net.Search.TopScoreDocCollector.Create(AppConst.MAX_COUNT_LIMIT, true);
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
                        if (!File.Exists(filePathField.StringValue))
                        {
                            // 该文件的在索引里的文档号,Doc是该文档进入索引时Lucene的编号，默认按照顺序编的
                            int docId = hit.Doc;
                            // 删除该索引
                            reader.DeleteDocument(docId);
                            reader.Commit();
                            continue;
                        }

                        log.Debug(fileNameField.StringValue + " => " + filePathField.StringValue + " ， " + fileSizeField.StringValue + " , " + createTimeField.StringValue);

                        // 文件信息
                        FileInfo fi = new FileInfo(filePathField.StringValue);

                        // 构造显示文件信息
                        Entity.FileInfo fileInfo = new Entity.FileInfo();
                        try
                        {
                            fileInfo.FileType = (FileType)System.Enum.Parse(typeof(FileType), fileTypeField.StringValue);
                        }
                        catch
                        {
                            fileInfo.FileType = FileType.文本文件;
                        }
                        fileInfo.FileName = fileNameField.StringValue;
                        fileInfo.FilePath = filePathField.StringValue;
                        fileInfo.Breviary = breviaryField.StringValue;
                        fileInfo.FileSize = long.Parse(fileSizeField.StringValue);
                        fileInfo.CreateTime = createTimeField.StringValue;
                        fileInfo.Keywords = keywords;

                        this.Dispatcher.BeginInvoke(new Action(() => {
                            FileInfoItem infoItem = new FileInfoItem(fileInfo);
                            infoItem.Tag = fileInfo;
                            this.SearchResultList.Items.Add(infoItem);
                        }));                        

                        resultNum++;
                    }

                    string msg = "检索完成：分词（" + text + "），共检索" + resultNum + "个符合条件的结果（只显示前" + AppConst.MAX_COUNT_LIMIT + "条），耗时：" + taskMark.ConsumeTime + "秒。";

                    this.Dispatcher.BeginInvoke(new Action(() => {
                        Message.ShowSuccess("MessageContainer", msg);
                    }));

                    ShowStatus(msg);
                }
                finally
                {
                    if (searcher != null)
                        searcher.Dispose();

                    if (reader != null)
                        reader.Dispose();
                }
            });
            t.Priority = ThreadPriority.Highest;
            t.Start();
        }
        #endregion

        #region 功能按钮事件
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

            // 搜索按钮时，下拉框和其他筛选条件全部恢复默认值
            this.MatchWords.IsChecked = false;
            this.OnlyFileName.IsChecked = false;
            this.FileTypeFilter.SelectedIndex = 0;

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

                // 搜索按钮时，下拉框和其他筛选条件全部恢复默认值
                this.MatchWords.IsChecked = false;
                this.OnlyFileName.IsChecked = false;
                this.FileTypeFilter.SelectedIndex = 0;

                BeforeSearch();

                SearchText.Focus();
            }
        }

        /// <summary>
        /// 文本内容变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // 搜索关键词
                string text = this.SearchText.Text;

                // 清除开始和结尾空格
                text = text.Trim();
                // 替换特殊字符
                text = AppConst.REGIX_SPECIAL_CHARACTER.Replace(text, "");

                // 回写处理过的字符
                this.SearchText.Text = text;

                // 光标定位到最后
                this.SearchText.SelectionStart = this.SearchText.Text.Length;
            } catch { }
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
        /// 仅文件名选中时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlyFileName_Checked(object sender, RoutedEventArgs e)
        {
            BeforeSearch();
        }

        /// <summary>
        /// 仅文件名取消选中时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlyFileName_Unchecked(object sender, RoutedEventArgs e)
        {
            BeforeSearch();
        }
        
        /// <summary>
        /// 匹配全瓷选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchWords_Checked(object sender, RoutedEventArgs e)
        {
            BeforeSearch();
        }

        /// <summary>
        /// 匹配全瓷取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchWords_Unchecked(object sender, RoutedEventArgs e)
        {
            BeforeSearch();
        }

        /// <summary>
        /// 列表项被选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchResultList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(SearchResultList.SelectedIndex == -1)
            {
                return;
            }

            // 手动GC
            GC.Collect();
            GC.WaitForPendingFinalizers();

            FileInfoItem infoItem = SearchResultList.SelectedItem as FileInfoItem;
            Entity.FileInfo fileInfo = infoItem.Tag as Entity.FileInfo;

            // 根据文件类型显示图标
            this.PreviewFileTypeIcon.Source = FileUtil.GetFileIcon(fileInfo.FileType);
            this.PreviewFileName.Text = fileInfo.FileName;
            this.PreviewFileContent.Document.Blocks.Clear();

            // 绑定打开文件和打开路径的Tag
            this.OpenFile.Tag = fileInfo.FilePath;
            this.OpenFolder.Tag = fileInfo.FilePath.Replace(fileInfo.FileName, "");

            // 判断文件大小，超过2m的文件不预览
            if (FileUtil.OutOfRange(fileInfo.FileSize))
            {
                Message.ShowInfo("MessageContainer", "只能预览小于2MB的文档");
                return;
            }

            // 获取扩展名
            string fileExt = Path.GetExtension(fileInfo.FilePath).Replace(".", "");

            // 图片文件
            if (FileType.常用图片.GetDescription().Contains(fileExt))
            {
                this.PreviewFileContent.Visibility = Visibility.Hidden;
                this.PreviewImage.Visibility = Visibility.Visible;
                Thread t = new Thread(new ThreadStart(() =>
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = new MemoryStream(File.ReadAllBytes(fileInfo.FilePath));
                    bi.EndInit();
                    bi.Freeze();

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.PreviewImage.Source = bi;
                    }));
                }));
                t.Priority = ThreadPriority.AboveNormal;
                t.Start();
            }
            else
            {
                this.PreviewImage.Visibility = Visibility.Hidden;
                this.PreviewFileContent.Visibility = Visibility.Visible;
                // 文件内容预览
                Thread t = new Thread(new ThreadStart(() =>
                {
                    string content = "";
                    if (CacheUtil.Exsits(fileInfo.FilePath))
                    {
                        content = CacheUtil.Get<string>(fileInfo.FilePath);
                    }
                    else
                    {
                        // 文件内容
                        content = FileInfoServiceFactory.GetFileInfoService(fileInfo.FileType).GetFileContent(fileInfo.FilePath);

                        // 写入缓存
                        CacheUtil.Add(fileInfo.FilePath, content);
                    }

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 填充数据
                        RichTextBoxUtil.FillingData(this.PreviewFileContent, content, new SolidColorBrush(Colors.Black));

                        // 关键词高亮
                        RichTextBoxUtil.Highlighted(this.PreviewFileContent, Colors.Red, fileInfo.Keywords);
                    }));
                }));
                t.Priority = ThreadPriority.AboveNormal;
                t.Start();
            }
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
            build = true;

            ShowStatus("开始更新索引...");

            BuildIndex(false);
        }

        /// <summary>
        /// 重建按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RebuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIndexExist(false))
            {
                var result = await MessageBoxR.ConfirmInContainer("DialogContaioner", "确定要重建索引嘛？时间可能比较久哦！", "提示");
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            if (build)
            {
                Message.ShowWarning("MessageContainer", "索引构建中，请稍等。");
                return;
            }
            build = true;

            ShowStatus("开始重建索引...");

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
            // 为空直接返回null
            if (string.IsNullOrEmpty(text)) return null;

            List<string> keywords = new List<string>();
            if (text.IndexOf(" ") != -1)
            {
                string[] texts = text.Split(' ');
                foreach (string txt in texts)
                {
                    if (string.IsNullOrEmpty(txt))
                    {
                        continue;
                    }
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

            this.OpenFile.Tag = null;
            this.OpenFolder.Tag = null;
            this.PreviewFileName.Text = "";
            this.PreviewFileContent.Document.Blocks.Clear();
            this.PreviewImage.Source = null;

            this.WorkStatus.Text = "就绪";
            this.OnlyFileName.IsChecked = false;
            this.MatchWords.IsChecked = false;
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
            /*if (build)
            {
                Message.ShowWarning("MessageContainer", "索引构建中，请稍等。");
                return;
            }*/

            // 清空预览信息
            this.OpenFile.Tag = null;
            this.OpenFolder.Tag = null;
            this.PreviewFileName.Text = "";
            this.PreviewFileContent.Document.Blocks.Clear();
            this.PreviewImage.Source = null;

            // 搜索
            Search(keywords, (filter == null ? null : filter + ""), (bool)this.OnlyFileName.IsChecked, (bool)this.MatchWords.IsChecked);
        }
        #endregion
    }
}
