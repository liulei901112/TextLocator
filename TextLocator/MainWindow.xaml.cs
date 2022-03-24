using log4net;
using Rubyer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TextLocator.Core;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.HotKey;
using TextLocator.Index;
using TextLocator.Util;

namespace TextLocator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 全部
        /// </summary>
        private RadioButton _radioButtonAll;
        /// <summary>
        /// 索引文件夹列表
        /// </summary>
        private List<string> _indexFolders = new List<string>();
        /// <summary>
        /// 排除文件夹列表
        /// </summary>
        private List<string> _exclusionFolders = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        private Regex _regexExclusionFolder;
        /// <summary>
        /// 时间戳
        /// </summary>
        private long _timestamp;

        /// <summary>
        /// 索引构建中
        /// </summary>
        private static volatile bool build = false;
        /// <summary>
        /// 当前页
        /// </summary>
        public int pageNow = 1;

        #region 热键
        /// <summary>
        /// 当前窗口句柄
        /// </summary>
        private IntPtr _hwnd = new IntPtr();
        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private Dictionary<HotKeySetting, int> _hotKeySettings = new Dictionary<HotKeySetting, int>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// WPF窗体的资源初始化完成，并且可以通过WindowInteropHelper获得该窗体的句柄用来与Win32交互后调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 获取窗体句柄
            _hwnd = new WindowInteropHelper(this).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(_hwnd);
            // 添加处理程序
            if (hWndSource != null) hWndSource.AddHook(WndProc);
        }

        /// <summary>
        /// 所有控件初始化完成后调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // 注册热键
            InitHotKey();
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

            // 初始化排序类型列表
            InitializeSortType();

            // 清理事件
            CleanSearchResult();

            // 检查配置参数信息
            if (string.IsNullOrEmpty(AppUtil.ReadValue("AppConfig", "ResultListPageSize", "")))
            {
                AppUtil.WriteValue("AppConfig", "ResultListPageSize", AppConst.MRESULT_LIST_PAGE_SIZE + "");
            }

            // 检查索引是否存在：如果存在才执行更新检查，不存在的跳过更新检查。
            if (CheckIndexExist())
            {
                // 软件每次启动时执行索引更新逻辑？
                CheckingIndexUpdates();
            }            

            // 注册全局热键时间
            HotKeySettingManager.Instance.RegisterGlobalHotKeyEvent += Instance_RegisterGlobalHotKeyEvent;
        }

        /// <summary>
        /// 窗口关闭中，改为隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// 窗口激活
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        #region 初始化

        /// <summary>
        /// 初始化排序类型列表
        /// </summary>
        private void InitializeSortType()
        {
            TaskTime taskTime = TaskTime.StartNew();
            Array sorts = Enum.GetValues(typeof(SortType));
            SortOptions.Items.Clear();
            foreach(var sort in sorts)
            {
                SortOptions.Items.Add(sort);
            }
            log.Debug("InitializeSortType 耗时：" + taskTime.ConsumeTime + "秒");
        }

        /// <summary>
        /// 初始化文件类型过滤器列表
        /// </summary>
        private void InitializeFileTypeFilters()
        {
            TaskTime taskTime = TaskTime.StartNew();
            // 文件类型筛选下拉框数据初始化
            FileTypeFilter.Children.Clear();
            FileTypeNames.Children.Clear();

            _radioButtonAll = new RadioButton()
            {
                GroupName = "FileTypeFilter",
                Width = 80,
                Margin = new Thickness(1),
                Tag = "全部",
                Content = "全部",
                Name = "FileTypeAll",
                IsChecked = true,
                ToolTip = "All"
            };
            _radioButtonAll.Checked += FileType_Checked;
            FileTypeFilter.Children.Add(_radioButtonAll);


            // 获取文件类型枚举，遍历并加入下拉列表
            foreach (FileType fileType in Enum.GetValues(typeof(FileType)))
            {
                RadioButton radioButton = new RadioButton()
                {
                    GroupName = "FileTypeFilter",
                    Width = 80,
                    Margin = new Thickness(1),
                    Tag = fileType.ToString(),
                    Content = fileType.ToString(),
                    Name = "FileType" + fileType.ToString(),
                    IsChecked = false,
                    ToolTip = fileType.GetDescription()
                };
                radioButton.Checked += FileType_Checked;
                FileTypeFilter.Children.Add(radioButton);

                // 标签
                FileTypeNames.Children.Add(new Button()
                {
                    Content = fileType.ToString(),
                    Height = 20,
                    Margin = new Thickness(2, 0, 0, 0),
                    ToolTip = fileType.GetDescription(),
                    Background = Brushes.DarkGray
                });
            }
            log.Debug("InitializeFileTypeFilters 耗时：" + taskTime.ConsumeTime + "秒");
        }

        /// <summary>
        /// 初始化配置文件信息
        /// </summary>
        private void InitializeAppConfig()
        {
            TaskTime taskTime = TaskTime.StartNew();
            // 初始化显示被索引的文件夹列表
            _indexFolders.Clear();

            // 读取被索引文件夹配置信息，如果配置信息为空：默认为我的文档和我的桌面
            string folderPaths = AppUtil.ReadValue("AppConfig", "FolderPaths", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "," + Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            // 配置信息不为空
            if (!string.IsNullOrEmpty(folderPaths))
            {
                string[] folderPathArray = folderPaths.Split(',');
                foreach (string folderPath in folderPathArray)
                {
                    _indexFolders.Add(folderPath);
                }
            }
            FolderPaths.Text = folderPaths;
            FolderPaths.ToolTip = FolderPaths.Text;

            // 读取排除文件夹，
            string exclusionPaths = AppUtil.ReadValue("AppConfig", "ExclusionPaths", "");
            if (!string.IsNullOrEmpty(exclusionPaths))
            {
                string[] exclusionPathArray = exclusionPaths.Split(',');
                foreach (string exclusionPath in exclusionPathArray)
                {
                    _exclusionFolders.Add(exclusionPath);
                }
                _regexExclusionFolder = new Regex(@"(" + exclusionPaths.Replace("\\", "\\\\").Replace(',', '|') + ")");
            }
            else
            {
                _regexExclusionFolder = null;
            }
            ExclusionPaths.Text = exclusionPaths;
            ExclusionPaths.ToolTip = ExclusionPaths.Text;            

            log.Debug("InitializeAppConfig 耗时：" + taskTime.ConsumeTime + "秒");
        }

        #endregion

        #region 热键
        /// <summary>
        /// 通知注册系统快捷键事件处理函数
        /// </summary>
        /// <param name="hotKeyModelList"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool Instance_RegisterGlobalHotKeyEvent(System.Collections.ObjectModel.ObservableCollection<HotKeyModel> hotKeyModelList)
        {
            InitHotKey(hotKeyModelList);
            return true;
        }

        /// <summary>
        /// 初始化注册快捷键
        /// </summary>
        /// <param name="hotKeyModelList">待注册热键的项</param>
        /// <returns>true:保存快捷键的值；false:弹出设置窗体</returns>
        private async Task<bool> InitHotKey(ObservableCollection<HotKeyModel> hotKeyModelList = null)
        {
            var list = hotKeyModelList ?? HotKeySettingManager.Instance.LoadDefaultHotKey();
            // 注册全局快捷键
            string failList = HotKeyHelper.RegisterGlobalHotKey(list, _hwnd, out _hotKeySettings);
            if (string.IsNullOrEmpty(failList))
                return true;

            var result = await MessageBoxR.ConfirmInContainer("DialogContaioner", string.Format("无法注册下列快捷键：\r\n\r\n{0}是否要改变这些快捷键？", failList), "提示", MessageBoxButton.YesNo);
            // 弹出热键设置窗体
            var win = HotkeyWindow.CreateInstance();
            if (result == MessageBoxResult.Yes)
            {
                if (!win.IsVisible)
                {
                    win.ShowDialog();
                }
                else
                {
                    win.Activate();
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 窗体回调函数，接收所有窗体消息的事件处理函数
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="msg">消息</param>
        /// <param name="wideParam">附加参数1</param>
        /// <param name="longParam">附加参数2</param>
        /// <param name="handled">是否处理</param>
        /// <returns>返回句柄</returns>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            var hotKeySetting = new HotKeySetting();
            switch (msg)
            {
                case HotKeyManager.WM_HOTKEY:
                    int sid = wideParam.ToInt32();
                    // 显示
                    if (sid == _hotKeySettings[HotKeySetting.显示])
                    {
                        hotKeySetting = HotKeySetting.显示;
                        
                        this.Show();
                        this.WindowState = WindowState.Normal;
                    }
                    // 隐藏
                    else if (sid == _hotKeySettings[HotKeySetting.隐藏])
                    {
                        hotKeySetting = HotKeySetting.隐藏;
                        this.Hide();
                    }
                    // 清空
                    else if (sid == _hotKeySettings[HotKeySetting.清空])
                    {
                        hotKeySetting = HotKeySetting.清空;
                        CleanSearchResult();
                    }
                    // 退出
                    else if (sid == _hotKeySettings[HotKeySetting.退出])
                    {
                        hotKeySetting = HotKeySetting.退出;
                        AppCore.Shutdown();
                    }
                    // 上一项
                    else if (sid == _hotKeySettings[HotKeySetting.上一个])
                    {
                        hotKeySetting = HotKeySetting.上一个;
                        Switch2Preview(HotKeySetting.上一个);
                    }
                    // 下一项
                    else if (sid == _hotKeySettings[HotKeySetting.下一个])
                    {
                        hotKeySetting = HotKeySetting.下一个;
                        Switch2Preview(HotKeySetting.下一个);
                    }
                    log.Debug(string.Format("触发【{0}】快捷键", hotKeySetting));
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region 搜索
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
            MatchWords.IsChecked = false;
            OnlyFileName.IsChecked = false;
            (this.FindName("FileTypeAll") as RadioButton).IsChecked = true;
            SortOptions.SelectedIndex = 0;

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
                // 光标移除文本框
                SearchText.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                // 搜索按钮时，下拉框和其他筛选条件全部恢复默认值
                MatchWords.IsChecked = false;
                OnlyFileName.IsChecked = false;
                (this.FindName("FileTypeAll") as RadioButton).IsChecked = true;
                SortOptions.SelectedIndex = 0;

                BeforeSearch();

                // 光标聚焦
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
                string text = SearchText.Text;

                // 替换特殊字符
                text = AppConst.REGEX_SPECIAL_CHARACTER.Replace(text, "");

                // 回写处理过的字符
                SearchText.Text = text;

                // 光标定位到最后
                SearchText.SelectionStart = SearchText.Text.Length;

                // 如果文本为空则隐藏清空按钮，如果不为空则显示清空按钮
                CleanButton.Visibility = text.Length > 0 ? Visibility.Visible : Visibility.Hidden;
            }
            catch { }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="fileType">文件类型</param>
        /// <param name="sortType">排序类型</param>
        /// <param name="onlyFileName">仅文件名</param>
        /// <param name="matchWords">匹配全词</param>
        private void Search(List<string> keywords, string fileType, SortType sortType, long timestamp, bool onlyFileName = false, bool matchWords = false)
        {
            if (!CheckIndexExist())
            {
                return;
            }

            Thread t = new Thread(() => {
                // 清空搜索结果列表
                Dispatcher.Invoke(new Action(() => {
                    SearchResultList.Items.Clear();
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
                    Lucene.Net.Search.BooleanQuery boolQuery = new Lucene.Net.Search.BooleanQuery();
                    for (int i = 0; i < keywords.Count; i++)
                    {
                        Lucene.Net.Search.Query query = parser.Parse(keywords[i]);
                        boolQuery.Add(query, onlyFileName || matchWords ? Lucene.Net.Search.Occur.MUST : Lucene.Net.Search.Occur.SHOULD);
                    }

                    // 文件类型筛选
                    if (!string.IsNullOrWhiteSpace(fileType))
                    {
                        boolQuery.Add(new Lucene.Net.Search.TermQuery(new Lucene.Net.Index.Term("FileType", fileType)), Lucene.Net.Search.Occur.MUST);
                    }

                    // 排序
                    Lucene.Net.Search.Sort sort = new Lucene.Net.Search.Sort();
                    switch (sortType)
                    {
                        case SortType.默认排序: break;
                        case SortType.从远到近:
                            // 按照CreateTime字段排序，false表示升序
                            sort.SetSort(new Lucene.Net.Search.SortField("CreateTime", Lucene.Net.Search.SortField.STRING_VAL, false));
                            break;
                        case SortType.从近到远:
                            sort.SetSort(new Lucene.Net.Search.SortField("CreateTime", Lucene.Net.Search.SortField.STRING_VAL, true));
                            break;
                        case SortType.从小到大:
                            sort.SetSort(new Lucene.Net.Search.SortField("FileSize", Lucene.Net.Search.SortField.INT, false));
                            break;
                        case SortType.从大到小:
                            sort.SetSort(new Lucene.Net.Search.SortField("FileSize", Lucene.Net.Search.SortField.INT, true));
                            break;
                    }


                    // 查询数据分页
                    Lucene.Net.Search.TopFieldDocs topDocs = searcher.Search(boolQuery, null, pageNow * PageSize, sort);
                    // 结果数组
                    Lucene.Net.Search.ScoreDoc[] scores = topDocs.ScoreDocs;

                    // 查询到的条数
                    int totalHits = topDocs.TotalHits;

                    // 设置分页标签总条数
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        // 如果总条数小于等于分页条数，则不显示分页
                        this.PageBar.Total = totalHits > PageSize ? totalHits : 0;

                        // 上一个和下一个切换面板是否显示
                        this.SwitchPreview.Visibility = totalHits > 0 ? Visibility.Visible : Visibility.Hidden;
                    }));

                    string msg = "检索完成。分词：( " + text + " )，结果：" + totalHits + "个符合条件的结果 (第 " + pageNow + " 页)，耗时：" + taskMark.ConsumeTime + "秒。";

                    log.Debug(msg);

                    ShowStatus(msg);

                    // 计算检索结果数量
                    // int resultNum = 0;

                    // 索引文档对象
                    Lucene.Net.Documents.Document doc;
                    // 文件信息
                    // FileInfo fi;
                    // 显示文件信息
                    Entity.FileInfo fileInfo;

                    // 计算显示数据
                    int start = (pageNow - 1) * PageSize;
                    int end = PageSize * pageNow;
                    if (end > totalHits) end = totalHits;
                    // 获取并显示列表
                    for (int i = start; i < end; i++)
                    {
                        if (_timestamp != timestamp)
                        {
                            continue;
                        }
                        // 该文件的在索引里的文档号,Doc是该文档进入索引时Lucene的编号，默认按照顺序编的
                        int docId = scores[i].Doc;
                        // 获取文档对象
                        doc = reader.Document(docId);

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
                            // 删除该索引
                            reader.DeleteDocument(docId);
                            reader.Commit();
                            continue;
                        }

                        log.Debug(fileNameField.StringValue + " => " + filePathField.StringValue + " ， " + fileSizeField.StringValue + " , " + createTimeField.StringValue);

                        // 文件信息
                        // fi = new FileInfo(filePathField.StringValue);

                        // 构造显示文件信息
                        fileInfo = new Entity.FileInfo()
                        {
                            FileName = fileNameField.StringValue,
                            FilePath = filePathField.StringValue,
                            Breviary = breviaryField.StringValue,
                            FileSize = long.Parse(fileSizeField.StringValue),
                            CreateTime = createTimeField.StringValue,
                            Keywords = keywords
                        };
                        try
                        {
                            fileInfo.FileType = (FileType)System.Enum.Parse(typeof(FileType), fileTypeField.StringValue);
                        }
                        catch
                        {
                            fileInfo.FileType = FileType.纯文本;
                        }

                        Dispatcher.Invoke(new Action(() => {
                            Entity.FileInfo fi = fileInfo;
                            SearchResultList.Items.Add(new FileInfoItem(fi));
                        }));
                        // resultNum++;
                    }
                }
                finally
                {
                    try
                    {
                        if (searcher != null)
                            searcher.Dispose();

                        if (reader != null)
                            reader.Dispose();
                    }
                    catch { }
                }
            });
            t.Priority = ThreadPriority.Highest;
            t.Start();
        }
        #endregion

        #region 分页
        /// <summary>
        /// 实现INotifyPropertyChanged接口
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// 每页显示数量
        /// </summary>
        public int PageSize
        {
            // 获取值时将私有字段传出；
            get { return AppConst.MRESULT_LIST_PAGE_SIZE; }
            set
            {
                // 赋值时将值传给私有字段
                AppConst.MRESULT_LIST_PAGE_SIZE = value;
                // 一旦执行了赋值操作说明其值被修改了，则立马通过INotifyPropertyChanged接口告诉UI(IntValue)被修改了
                OnPropertyChanged("PageSize");
            }
        }

        private void PageBar_PageIndexChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            log.Debug($"pageIndex : {e.OldValue} => {e.NewValue}");

            // 搜索按钮时，下拉框和其他筛选条件全部恢复默认值
            MatchWords.IsChecked = false;
            OnlyFileName.IsChecked = false;
            (this.FindName("FileTypeAll") as RadioButton).IsChecked = true;

            BeforeSearch(e.NewValue);
        }

        private void PageBar_PageSizeChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            log.Debug($"pageSize : {e.OldValue} => {e.NewValue}");
        }
        #endregion

        #region 排序
        /// <summary>
        /// 排序选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BeforeSearch(pageNow);
        }
        #endregion

        #region 清空
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
        /// 清理查询结果
        /// </summary>
        private void CleanSearchResult()
        {   
            // 搜索结果列表清空
            SearchResultList.Items.Clear();

            // 右侧预览区，打开文件和文件夹标记清空
            OpenFile.Tag = null;
            OpenFolder.Tag = null;

            // 预览文件名清空
            PreviewFileName.Text = "";
            
            // 预览文件内容清空
            PreviewFileContent.Document.Blocks.Clear();

            // 预览图片清空
            PreviewImage.Source = null;

            // 预览文件类型图标清空
            PreviewFileTypeIcon.Source = null;

            // 仅文件名 和 全词匹配取消选中
            OnlyFileName.IsChecked = false;
            MatchWords.IsChecked = false;

            // 文件类型筛选取消选中
            ToggleButtonAutomationPeer toggleButtonAutomationPeer = new ToggleButtonAutomationPeer(_radioButtonAll);
            IToggleProvider toggleProvider = toggleButtonAutomationPeer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
            toggleProvider.Toggle();

            // 还原为第一页
            pageNow = 1;
            // 设置分页标签总条数
            this.Dispatcher.BeginInvoke(new Action(() => {
                this.PageBar.Total = 0;
                this.PageBar.PageIndex = 1;
            }));

            // 排序类型切换为默认
            this.SortOptions.SelectedIndex = 0;

            // 隐藏上一个和下一个切换面板
            this.SwitchPreview.Visibility = Visibility.Collapsed;


            SearchText.Text = "";
            // 光标移除文本框
            SearchText.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            // 光标聚焦
            SearchText.Focus();

            // 工作状态更新为就绪
            WorkStatus.Text = "就绪";
        }
        #endregion

        #region 列表
        /// <summary>
        /// 列表项被选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultList.SelectedIndex == -1)
            {
                return;
            }

            // 预览切换索引标记
            this.SwitchPreview.Tag = SearchResultList.SelectedIndex;

            // 手动GC
            GC.Collect();
            GC.WaitForPendingFinalizers();

            FileInfoItem infoItem = SearchResultList.SelectedItem as FileInfoItem;
            Entity.FileInfo fileInfo = infoItem.Tag as Entity.FileInfo;

            // 根据文件类型显示图标
            PreviewFileTypeIcon.Source = FileUtil.GetFileIcon(fileInfo.FileType);
            PreviewFileName.Text = fileInfo.FileName;
            PreviewFileContent.Document.Blocks.Clear();

            // 绑定打开文件和打开路径的Tag
            OpenFile.Tag = fileInfo.FilePath;
            OpenFolder.Tag = fileInfo.FilePath.Replace(fileInfo.FileName, "");

            // 判断文件大小，超过2m的文件不预览
            if (FileUtil.OutOfRange(fileInfo.FileSize))
            {
                Message.ShowInfo("MessageContainer", "只能预览小于2MB的文档");
                return;
            }

            // 获取扩展名
            string fileExt = Path.GetExtension(fileInfo.FilePath).Replace(".", "");

            // 图片文件
            if (FileType.图片.GetDescription().Contains(fileExt))
            {
                PreviewFileContent.Visibility = Visibility.Hidden;
                PreviewImage.Visibility = Visibility.Visible;
                Thread t = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = new MemoryStream(File.ReadAllBytes(fileInfo.FilePath));
                        bi.EndInit();
                        bi.Freeze();

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            PreviewImage.Source = bi;
                        }));
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                        try
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                PreviewImage.Source = null;
                            }));
                        }
                        catch { }
                    }
                }));
                t.Priority = ThreadPriority.AboveNormal;
                t.Start();
            }
            else
            {
                PreviewImage.Visibility = Visibility.Hidden;
                PreviewFileContent.Visibility = Visibility.Visible;
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
                        content = FileInfoServiceFactory.GetFileContent(fileInfo.FilePath);

                        // 写入缓存
                        CacheUtil.Add(fileInfo.FilePath, content);
                    }

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 填充数据
                        RichTextBoxUtil.FillingData(PreviewFileContent, content, new SolidColorBrush(Colors.Black));

                        ThreadPool.QueueUserWorkItem(_ => {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                // 关键词高亮
                                RichTextBoxUtil.Highlighted(PreviewFileContent, Colors.Red, fileInfo.Keywords);
                            }));
                        });
                    }));
                }));
                t.Priority = ThreadPriority.AboveNormal;
                t.Start();
            }
        }
        #endregion

        #region 界面事件
        /// <summary>
        /// 文件类型过滤器选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileType_Checked(object sender, RoutedEventArgs e)
        {
            FileTypeFilter.Tag = (sender as RadioButton).Content;
            
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
        /// 优化按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndexUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (build)
            {
                Message.ShowWarning("MessageContainer", "索引构建中，不能重复执行！");
                return;
            }
            build = true;

            ShowStatus("开始更新索引，请稍等...");

            BuildIndex(false);
        }

        /// <summary>
        /// 重建按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void IndexRebuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (build)
            {
                Message.ShowWarning("MessageContainer", "索引构建中，不能重复执行！");
                return;
            }
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

            ShowStatus("开始重建索引，请稍等...");

            BuildIndex(true);
        }

        /// <summary>
        /// 搜索区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderPaths_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AreaWindow areaDialog = new AreaWindow();
            areaDialog.ShowDialog();
            if (areaDialog.DialogResult == true)
            {
                InitializeAppConfig();
            }
        }

        /// <summary>
        /// 上一个
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLast_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Switch2Preview(HotKeySetting.上一个);
        }

        /// <summary>
        /// 下一个
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNext_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Switch2Preview(HotKeySetting.下一个);
        }

        /// <summary>
        /// 切换预览，next为true，下一个；next为false，上一个
        /// </summary>
        /// <param name="next"></param>
        private void Switch2Preview(HotKeySetting setting)
        {
            // 当前索引 = 预览标记不为空 ? 使用标记 ： 默认值0
            int index = this.SwitchPreview.Tag != null ? int.Parse(this.SwitchPreview.Tag + "") : -1;

            // 搜索结果列表为空时，不能执行切换
            if (this.SearchResultList.Items.Count <= 0)
            {
                return;
            }

            // 下一个
            if (setting == HotKeySetting.下一个 && index < this.SearchResultList.Items.Count)
            {
                this.SearchResultList.SelectedIndex = index + 1;
            }
            // 上一个
            else if (setting == HotKeySetting.上一个 && index > 0)
            {
                this.SearchResultList.SelectedIndex = index - 1;
            }
        }

        #endregion

        #region 辅助方法
        /// <summary>
        /// 检查索引是否需要更新
        /// </summary>
        private void CheckingIndexUpdates()
        {
            string lastIndexTime = AppUtil.ReadValue("AppConfig", "LastIndexTime", "");
            // 时间不存在 或 已经超过7天
            if (string.IsNullOrEmpty(lastIndexTime) || (DateTime.Now - DateTime.Parse(lastIndexTime)).TotalDays > 7)
            {
                // 执行索引更新，扫描新文件。
                BuildIndex();
            }
        }

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
                    Message.ShowWarning("MessageContainer", "首次使用，需先设置搜索区，并重建索引");
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
            // 提示语
            string tips = rebuild ? "重建" : "更新";

            Task.Factory.StartNew(() =>
            {
                var taskMark = TaskTime.StartNew();

                var fileMark = TaskTime.StartNew();

                ShowStatus("开始" + tips + "索引，正在扫描文件...");

                // 定义文件列表
                List<string> filePaths = new List<string>();
                foreach (string s in _indexFolders)
                {
                    log.Debug("目录：" + s);
                    // 获取文件信息列表
                    FileUtil.GetAllFiles(filePaths, _regexExclusionFolder, s);
                }
                log.Debug("GetFiles 耗时：" + fileMark.ConsumeTime + "秒");
                ShowStatus("文件扫描完成，开始" + tips + "索引...");

                // 验证
                if (filePaths == null || filePaths.Count <= 0)
                {
                    build = false;

                    ShowStatus("就绪");

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Message.ShowWarning("MessageContainer", "未找到可以需要索引的文档");
                    }));
                    
                    return;
                }

                // 排重
                filePaths = filePaths.Distinct().ToList();

                // 排序
                filePaths = ListUtil.Shuffle(filePaths);                

                // 创建索引方法
                IndexCore.CreateIndex(filePaths, rebuild, ShowStatus);

                string msg = "索引" + tips + "完成。共用时：" + taskMark.ConsumeTime + "秒";

                // 显示状态
                ShowStatus(msg);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Message.ShowSuccess("MessageContainer", msg);
                }));

                AppUtil.WriteValue("AppConfig", "LastIndexTime", DateTime.Now.ToString());

                // 构建结束
                build = false;
            });
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        /// <param name="text">消息</param>
        /// <param name="percent">进度</param>
        private void ShowStatus(string text, double percent = AppConst.MAX_PERCENT)
        {
            Dispatcher.BeginInvoke(new Action(() => {

                WorkStatus.Text = text;
                if (percent > AppConst.MIN_PERCENT)
                {
                    WorkProgress.Value = percent;

                    TaskbarItemInfo.ProgressState = percent < AppConst.MAX_PERCENT ? System.Windows.Shell.TaskbarItemProgressState.Normal : System.Windows.Shell.TaskbarItemProgressState.None;
                    TaskbarItemInfo.ProgressValue = WorkProgress.Value / WorkProgress.Maximum;
                }
            }));
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
            if (OpenFile.Tag != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(OpenFile.Tag + "");
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
            if (OpenFolder.Tag != null)
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", @"" + OpenFolder.Tag);
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
            string text = SearchText.Text.Trim();
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
        /// 搜索前
        /// </summary>
        /// <param name="page">指定页</param>
        private void BeforeSearch(int page = 1)
        {
            // 还原分页count
            if (page != pageNow) {
                pageNow = page;
                // 设置分页标签总条数
                this.Dispatcher.BeginInvoke(new Action(() => {
                    this.PageBar.Total = 0;
                    this.PageBar.PageIndex = pageNow;
                }));
            }

            object filter = FileTypeFilter.Tag;
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

            // 预览区打开文件和文件夹标记清空
            OpenFile.Tag = null;
            OpenFolder.Tag = null;

            // 预览文件名清空
            PreviewFileName.Text = "";

            // 预览文件内容清空
            PreviewFileContent.Document.Blocks.Clear();

            // 预览图标清空
            PreviewImage.Source = null;

            // 预览文件类型图标清空
            PreviewFileTypeIcon.Source = null;

            // 预览切换标记清空
            SwitchPreview.Tag = null;

            // 记录时间戳
            _timestamp = Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);

            // 搜索
            Search(
                keywords, 
                filter == null ? null : filter + "",
                (SortType)SortOptions.SelectedValue,
                _timestamp,
                (bool)OnlyFileName.IsChecked, 
                (bool)MatchWords.IsChecked
            );
        }
        #endregion
    }
}
