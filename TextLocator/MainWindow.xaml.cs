using log4net;
using Rubyer;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TextLocator.Enum;
using TextLocator.Factory;
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
        /// 应用目录
        /// </summary>
        private static readonly string _AppDir = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// App.ini路径：_AppDir\\_AppName\\Index\\
        /// </summary>
        private static readonly string _AppIndexDir = System.IO.Path.Combine(_AppDir, "index");
        /// <summary>
        /// 分词器
        /// new Lucene.Net.Analysis.Cn.ChineseAnalyzer();
        /// new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);// 用standardAnalyzer分词器
        /// </summary>
        private static readonly Lucene.Net.Analysis.Analyzer _AppIndexAnalyzer = new Lucene.Net.Analysis.PanGuAnalyzer();

        /// <summary>
        /// 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
        /// 磁盘路径：Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(_AppIndexDir))
        /// 内存：new Lucene.Net.Store.RAMDirectory()
        /// </summary>
        private static readonly Lucene.Net.Store.RAMDirectory _IndexDirctory = new Lucene.Net.Store.RAMDirectory();

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
            /*Task.Factory.StartNew(() =>
            {
                GetAllFiles("D:\\桌面文档");
            });

            Random random = new Random();
            this.SearchResultList.Items.Clear();
            for (int i = 0; i < 50; i++)
            {
                this.SearchResultList.Items.Add(new FileInfoItem(new Entity.FileInfo()
                {
                    FileName = "fileName" + i,
                    FilePath = "D:\\桌面文档\\金壕软件科技\\教 师 护 导 要 求（20210219）.doc",
                    FileSize = random.Next(10, 10000),
                    Content = "项目1、乙方为甲方网站的服务器进行维护和管理，包括：病毒排查和清除、网站数据的备份，网站空间的定期清理和维护，服务器帐号的管理等服务，保证网站的正常运行和网站数据的完整性以及网站服务器帐号的安全。 项目2、乙方为甲方网站的系统程序进行维护和管理，包括：程序代码的整理和优化、排除程序代码的漏洞、排除因系统错误导致的故障、保证网站系统的正常运行。",
                    CreateTime = DateTime.Now.ToString("yyyy-MM-dd")
                }));
            }*/


            _IndexFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            _IndexFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            string customFolders = AppUtil.ReadIni("Folders", "FolderPaths", "");
            if (!string.IsNullOrEmpty(customFolders))
            {
                string[] customFolderArray = customFolders.Split(',');
                foreach (string folderPath in customFolderArray) {
                    _IndexFolders.Add(folderPath);
                }
            }
            //_IndexFolders.Add("E:\\马士兵教育、潭州学院");
            _IndexFolders.Add("E:\\幼小衔接启蒙资料");
            _IndexFolders.Add("E:\\USB殷丹");
            string foldersText = "";
            foreach(string folder in _IndexFolders)
            {
                foldersText += folder + ", ";
            }
            this.Folders.Text = foldersText.Substring(0, foldersText.Length - 2);
            this.Folders.ToolTip = this.Folders.Text;

            // 清理事件
            Clean();

            Task.Factory.StartNew(() =>
            {
                foreach (string s in _IndexFolders)
                {
                    GetAllFiles(s);
                }                
            });
        }

        /// <summary>
        /// 获取全部文件
        /// </summary>
        /// <param name="rootPath"></param>
        private void GetAllFiles(string rootPath)
        {
            log.Debug("根目录：" + rootPath);
            // 声明一个files包，用来存储遍历出的word文档
            List<FileInfo> files = new List<FileInfo>();
            // 获取全部文件列表
            GetAllFiles(rootPath, files);
            // 创建索引方法
            CreateIndex(files);
        }

        /// <summary>
        /// 获取指定根目录下的子目录及其文档
        /// </summary>
        /// <param name="rootPath">根目录路径</param>
        /// <param name="files">word文档存储包</param>
        private void GetAllFiles(string rootPath, List<FileInfo> files)
        {
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            // 得到所有子目录
            try
            {
                string[] dirs = System.IO.Directory.GetDirectories(rootPath);
                foreach (string di in dirs)
                {
                    // 递归调用
                    GetAllFiles(di, files);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            // 查找word文件
            FileInfo[] file = dir.GetFiles("*.doc?");
            // 遍历每个word文档
            foreach (FileInfo fi in file)
            {
                files.Add(fi);
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="files">获得的文档包</param>
        private void CreateIndex(List<FileInfo> files)
        {
            bool isCreate = false;
            //判断是创建索引还是增量索引
            if (!Directory.Exists(_AppIndexDir))
            {
                isCreate = true;
            }

            // 索引写入初始化（FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上）
            Lucene.Net.Index.IndexWriter writer = new Lucene.Net.Index.IndexWriter(_IndexDirctory, _AppIndexAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

            // 遍历读取文件，并创建索引
            for (int i = 0; i < files.Count(); i++)
            {
                FileInfo fi = files[i];

                // 文件类型
                FileType fileType = FileType.Word类型;

                // 获取扩展名
                string extensionName = Path.GetFileNameWithoutExtension(fi.DirectoryName + "\\" + fi.Name);

                // 获取索引文档
                Lucene.Net.Documents.Document doc = FileInfoServiceFactory.GetFileInfoService(fileType).GetIndexDocument(fi);

                // 文件标记
                string fileMark = fi.DirectoryName + fi.CreationTime.ToString();

                // 当索引文件中含有与filemark相等的field值时，会先删除再添加，以防出现重复
                writer.DeleteDocuments(new Lucene.Net.Index.Term("FileMark", fileMark));
                // 不分词建索引
                doc.Add(new Lucene.Net.Documents.Field("FileMark", fileMark, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("FileType", fileType.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                writer.AddDocument(doc);
                // 优化索引
                writer.Optimize();
            }
            writer.Dispose();
        }

        /// <summary>
        /// 检索关键字
        /// </summary>
        /// <param name="keywords">关键字包</param>
        private void Search(List<string> keywords)
        {
            // 清空搜索结果列表
            this.SearchResultList.Items.Clear();

            int num = 10;
            if (keywords.Count != 0)
            {
                Lucene.Net.Index.IndexReader reader = null;
                Lucene.Net.Search.IndexSearcher searcher = null;
                try
                {
                    /*if (!System.IO.Directory.Exists(_AppIndexDir))
                    {
                        MessageBox.Show("首次使用该软件检索 必须先创建索引！" + "\r\n" + "请点击右边【创建索引】按钮,选择要检索的文件夹进行创建索引。");
                        return;
                    }*/
                    reader = Lucene.Net.Index.IndexReader.Open(_IndexDirctory, false);
                    searcher = new Lucene.Net.Search.IndexSearcher(reader);

                    // 创建查询
                    Lucene.Net.Analysis.PerFieldAnalyzerWrapper wrapper = new Lucene.Net.Analysis.PerFieldAnalyzerWrapper(_AppIndexAnalyzer);
                    wrapper.AddAnalyzer("FileName", _AppIndexAnalyzer);
                    wrapper.AddAnalyzer("FilePath", _AppIndexAnalyzer);
                    wrapper.AddAnalyzer("Content", _AppIndexAnalyzer);

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
            Clean();
        }

        /// <summary>
        /// 清理事件
        /// </summary>
        private void Clean()
        {
            this.SearchText.Text = "";
            this.SearchResultList.Items.Clear();
            this.PreviewFileContent.Document.Blocks.Clear();
        }
    }
}
