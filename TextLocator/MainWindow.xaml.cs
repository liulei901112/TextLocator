using log4net;
using Rubyer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using TextLocator.Consts;
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
            // 初始化元素数据
            InitializeElementData();

            // 清理事件
            CleanSearchResult();

            // 检查索引是否存在
            CheckIndexExist();
        }

        /// <summary>
        /// 初始化文件类型下拉框
        /// </summary>
        private void InitializeElementData()
        {
            // 文件类型筛选下拉框数据初始化
            this.FileTypeFilter.Items.Clear();
            this.FileTypeFilter.Items.Add("全部");
            // 获取文件类型枚举，遍历并加入下拉列表
            foreach(string fileTypeName in FileTypeUtil.GetFileTypes())
            {
                this.FileTypeFilter.Items.Add(fileTypeName);
            }
            // 默认选中全部
            this.FileTypeFilter.Text = "全部";

            // 初始化显示被索引的文件夹列表
            _IndexFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            _IndexFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            string customFolders = AppUtil.ReadIni("Folders", "FolderPaths", "");
            if (!string.IsNullOrEmpty(customFolders))
            {
                string[] customFolderArray = customFolders.Split(',');
                foreach (string folderPath in customFolderArray)
                {
                    _IndexFolders.Add(folderPath);
                }
            }
            //_IndexFolders.Add("E:\\马士兵教育、潭州学院");
            _IndexFolders.Add("E:\\幼小衔接启蒙资料");
            _IndexFolders.Add("E:\\USB殷丹");
            string foldersText = "";
            foreach (string folder in _IndexFolders)
            {
                foldersText += folder + ", ";
            }
            this.Regions.Text = foldersText.Substring(0, foldersText.Length - 2);
            this.Regions.ToolTip = this.Regions.Text;


            // 初始化支持文件后缀列表
            this.FileExtensions.Text = AppConst.FILE_EXTENSIONS;
            this.FileExtensions.ToolTip = this.FileExtensions.Text;
        }

        /// <summary>
        /// 检查索引是否存在
        /// </summary>
        /// <returns></returns>
        private bool CheckIndexExist()
        {
            bool exists = Directory.Exists(AppConst.APP_INDEX_DIR);
            if (!exists)
            {
                Message.ShowWarning("", "首次使用该软件，需先设置需要索引的文件夹。并点击右侧重建按钮进行初始化");
            }
            return exists;
        }

        /// <summary>
        /// 构建索引
        /// </summary>
        /// <param name="rebuild">重建，默认是优化</param>
        private void BuildIndex(bool rebuild = false)
        {
            if (build)
            {
                Message.ShowWarning("索引构建中，请稍等。");
                return;
            }
            build = true;

            Task.Factory.StartNew(() =>
            {
                DateTime beginMark = DateTime.Now;
                foreach (string s in _IndexFolders)
                {
                    // 获取文件信息列表
                    List<FileInfo> files = FileUtil.GetAllFiles(s);
                    // 创建索引方法
                    LuceneIndexCore.CreateIndex(files, rebuild, ShowStatus);
                }

                // 显示状态
                ShowStatus("索引执行结束，共用时：" + (DateTime.Now - beginMark).TotalSeconds + "秒");
            });            
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        /// <param name="text"></param>
        private void ShowStatus(string text)
        {
            this.Dispatcher.InvokeAsync(() => {
                this.WorkStatus.Text = text;
            });
        }

        /// <summary>
        /// 检索关键字
        /// </summary>
        /// <param name="keywords">关键字包</param>
        private void Search(List<string> keywords)
        {
            if (build)
            {
                Message.ShowWarning("索引构建中，请稍等。");
                return;
            }

            // 清空搜索结果列表
            this.SearchResultList.Items.Clear();

            int num = 10;
            if (keywords.Count != 0)
            {
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

                    // 创建查询
                    Lucene.Net.Analysis.PerFieldAnalyzerWrapper wrapper = new Lucene.Net.Analysis.PerFieldAnalyzerWrapper(AppConst.INDEX_ANALYZER);
                    wrapper.AddAnalyzer("FileName", AppConst.INDEX_ANALYZER);
                    wrapper.AddAnalyzer("FilePath", AppConst.INDEX_ANALYZER);
                    wrapper.AddAnalyzer("Content", AppConst.INDEX_ANALYZER);

                    string[] fields = { "FileName", "FilePath", "Content" };
                    Lucene.Net.QueryParsers.QueryParser parser = new Lucene.Net.QueryParsers.MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields, wrapper);
                    Lucene.Net.Search.BooleanQuery Bquery = new Lucene.Net.Search.BooleanQuery();
                    for (int i = 0; i < keywords.Count; i++)
                    {
                        Lucene.Net.Search.Query query = parser.Parse(keywords[i]);
                        Bquery.Add(query, Lucene.Net.Search.Occur.MUST);
                    }

                    Lucene.Net.Search.TopScoreDocCollector collector = Lucene.Net.Search.TopScoreDocCollector.Create(num, true);
                    searcher.Search(Bquery, collector);
                    var hits = collector.TopDocs().ScoreDocs;
                    // 以后就可以对获取到的collector数据进行操作
                    int resultNum = 0; //计算检索结果数量
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


                        log.Debug(fileNameField.StringValue + " = " + filePathField.StringValue + " ， " + fileSizeField.StringValue + " , " + createTimeField.StringValue);

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
                        infoItem.MouseUp += InfoItem_MouseUp;
                        infoItem.Tag = fileInfo;
                        this.SearchResultList.Items.Add(infoItem);

                        resultNum++;
                    }
                    Message.ShowSuccess("检索完成！共检索到" + resultNum + "个符合条件的结果！");
                }
                finally
                {
                    if (searcher != null)
                        searcher.Dispose();

                    if (reader != null)
                        reader.Dispose();
                }
            }
            else
            {
                Message.Show("请输入要查询的关键字!");
                return;
            }
        }

        /// <summary>
        /// 搜索结果列表点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoItem_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FileInfoItem infoItem = sender as FileInfoItem;
            Entity.FileInfo fileInfo = infoItem.Tag as Entity.FileInfo;

            this.PreviewFileName.Text = fileInfo.FileName;
            this.PreviewFileContent.Document.Blocks.Clear();

            // 文件内容
            string content = FileInfoServiceFactory.GetFileInfoService(fileInfo.FileType).GetFileContent(fileInfo.FilePath);

            // Paragraph 类似于 html 的 P 标签
            System.Windows.Documents.Paragraph p = new System.Windows.Documents.Paragraph();
            // Run 是一个 Inline 的标签
            Run r = new Run(content);
            p.Inlines.Add(r);
            this.PreviewFileContent.Document.Blocks.Add(p);

            // 关键词高亮
            RichTextBoxUtil.Highlighted(this.PreviewFileContent, Colors.Red, fileInfo.Keywords);
        }


        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string text = this.SearchText.Text;
            if (string.IsNullOrEmpty(text))
            {
                Message.Show("请输入搜索关键词");
                return;
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

            Search(keywords);
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
        /// 清理查询结果
        /// </summary>
        private void CleanSearchResult()
        {
            this.SearchText.Text = "";
            this.SearchResultList.Items.Clear();
            this.PreviewFileContent.Document.Blocks.Clear();
        }

        /// <summary>
        /// 优化按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptimizeButton_Click(object sender, RoutedEventArgs e)
        {
            BuildIndex(false);
        }

        /// <summary>
        /// 重建按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RebuildButton_Click(object sender, RoutedEventArgs e)
        {
            BuildIndex(true);
        }

        /// <summary>
        /// 搜索区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Regions_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RegionWindow region = new RegionWindow();
            region.ShowDialog();
        }
    }
}
