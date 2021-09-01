using log4net;
using Rubyer;
using Spire.Doc;
using Spire.Doc.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        /// 应用程序名称
        /// </summary>
        private static readonly string _AppName = Process.GetCurrentProcess().ProcessName.Replace(".exe", "");
        /// <summary>
        /// App.ini路径：_AppDir\\_AppName\\Index\\
        /// </summary>
        private static readonly string _AppIndexDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index");
        /// <summary>
        /// Analyzer analyzer = new Lucene.Net.Analysis.Cn.ChineseAnalyzer();
        /// </summary>
        private static readonly Lucene.Net.Analysis.Analyzer _AppIndexAnalyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);// 用standardAnalyzer分词器


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
            if (!Directory.Exists(_AppIndexDir))
            {
                Directory.CreateDirectory(_AppIndexDir);
            }

            Task.Factory.StartNew(() => {
                GetAllFiles("C:\\Users\\liulei\\Desktop\\新建文件夹");
            });
        }

        private void GetAllFiles(string rootPath)
        {
            log.Debug("根目录：" + rootPath);
            //声明一个files包，用来存储遍历出的word文档
            List<FileInfo> files = new List<FileInfo>(); 
            if (!System.IO.Directory.Exists(rootPath))
            {
                Message.Show("指定的目录不存在");
                return;
            }
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
            string[] dirs = System.IO.Directory.GetDirectories(rootPath);
            foreach (string di in dirs)
            {
                // 递归调用
                GetAllFiles(di, files); 
            }
            // 查找word文件
            FileInfo[] file = dir.GetFiles("*.doc?"); 
            // 遍历每个word文档
            foreach (FileInfo fi in file)
            {
                string filename = fi.Name;
                string filePath = fi.FullName;
                object filepath = filePath;
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
            // FSDirectory表示索引存放在硬盘上，RAMDirectory表示放在内存上
            //Lucene.Net.Index.IndexWriter writer = new Lucene.Net.Index.IndexWriter(Lucene.Net.Store.FSDirectory.Open(_AppIndexDir), _AppIndexAnalyzer, isCreate, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);
            for (int i = 0; i < files.Count(); i++)
            {
                /*//读取word文档内容
                Microsoft.Office.Interop.Word.ApplicationClass wordapp = new Microsoft.Office.Interop.Word.ApplicationClass();
                string filename = files[i].Name;
                object file = files[i].DirectoryName + "\\" + filename;
                object isreadonly = true;
                object nullobj = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Word._Document doct = wordapp.Documents.Open(ref file, ref nullobj, ref isreadonly, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj, ref nullobj);
                //doct.ActiveWindow.Selection.WholeStory();
                //doct.ActiveWindow.Selection.Copy();
                //IDataObject data = Clipboard.GetDataObject();
                ////读出的内容赋给content变量
                //string content = data.GetData(DataFormats.Text).ToString();
                string content = doct.Content.Text;
                FileInfo fi = new FileInfo(file.ToString());
                string createTime = fi.CreationTime.ToString();
                string filemark = files[i].DirectoryName + createTime;
                // 关闭word
                object missingValue = Type.Missing;
                object miss = System.Reflection.Missing.Value;
                object saveChanges = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                doct.Close(ref saveChanges, ref missingValue, ref missingValue);
                wordapp.Quit(ref saveChanges, ref miss, ref miss);
                //  StreamReader reader = new StreamReader(fileInfo.FullName);读取txt文件的方法，如读word会出现乱码，不适用于word的读取
                Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();*/
                string filename = files[i].Name;
                object file = files[i].DirectoryName + "\\" + filename;
                FileInfo fi = new FileInfo(file.ToString());
                string createTime = fi.CreationTime.ToString();
                string filemark = files[i].DirectoryName + createTime;
                log.Debug(file);

                string content = "";

                using (var document = new Document(new FileStream(file.ToString(), FileMode.Open)))
                {
                    // 提取每个段落的文本 
                    var sb = new StringBuilder();
                    foreach (Section section in document.Sections)
                    {
                        foreach (Paragraph paragraph in section.Paragraphs)
                        {
                            sb.AppendLine(paragraph.Text);
                        }
                    }
                    content = sb.ToString();
                }

                Console.WriteLine(content);

                /*Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
                // 当索引文件中含有与filemark相等的field值时，会先删除再添加，以防出现重复
                writer.DeleteDocuments(new Lucene.Net.Index.Term("filemark", filemark));
                // 不分词建索引
                doc.Add(new Lucene.Net.Documents.Field("filemark", filemark, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
                // ANALYZED分词建索引
                doc.Add(new Lucene.Net.Documents.Field("FileName", filename, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("Content", content, Lucene.Net.Documents.Field.Store.NO, Lucene.Net.Documents.Field.Index.ANALYZED));
                doc.Add(new Lucene.Net.Documents.Field("Path", file.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                writer.AddDocument(doc);
                // 优化索引
                writer.Optimize(); */
            }
            //writer.Dispose();
        }

        /// <summary>
        /// 检索关键字
        /// </summary>
        /// <param name="strKey">关键字包</param>
        private void SearchKey(List<string> strKey)
        {
            int num = 10;
            if (strKey.Count != 0)
            {
                Lucene.Net.Index.IndexReader reader = null;
                Lucene.Net.Search.IndexSearcher searcher = null;
                try
                {
                    if (!System.IO.Directory.Exists(_AppIndexDir))
                    {
                        MessageBox.Show("首次使用该软件检索 必须先创建索引！" + "\r\n" + "请点击右边【创建索引】按钮,选择要检索的文件夹进行创建索引。");
                        return;
                    }
                    reader = Lucene.Net.Index.IndexReader.Open(Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(_AppIndexDir)), false);
                    searcher = new Lucene.Net.Search.IndexSearcher(reader);

                    //创建查询
                    Lucene.Net.Analysis.PerFieldAnalyzerWrapper wrapper = new Lucene.Net.Analysis.PerFieldAnalyzerWrapper(_AppIndexAnalyzer);
                    wrapper.AddAnalyzer("FileName", _AppIndexAnalyzer);
                    wrapper.AddAnalyzer("Content", _AppIndexAnalyzer);
                    wrapper.AddAnalyzer("Path", _AppIndexAnalyzer);
                    string[] fields = { "FileName", "Content", "Path" };
                    Lucene.Net.QueryParsers.QueryParser parser = new Lucene.Net.QueryParsers.MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields, wrapper);
                    Lucene.Net.Search.BooleanQuery Bquery = new Lucene.Net.Search.BooleanQuery();
                    for (int i = 0; i < strKey.Count; i++)
                    {
                        Lucene.Net.Search.Query query = parser.Parse(strKey[i]);
                        Bquery.Add(query, Lucene.Net.Search.Occur.MUST);
                    }

                    Lucene.Net.Search.TopScoreDocCollector collector = Lucene.Net.Search.TopScoreDocCollector.Create(num, true);
                    searcher.Search(Bquery, collector);
                    var hits = collector.TopDocs().ScoreDocs;
                    //以后就可以对获取到的collector数据进行操作
                    int resultNum = 0; //计算检索结果数量
                    for (int i = 0; i < hits.Count(); i++)
                    {
                        var hit = hits[i];
                        Lucene.Net.Documents.Document doc = searcher.Doc(hit.Doc);
                        Lucene.Net.Documents.Field fileNameField = doc.GetField("FileName");
                        Lucene.Net.Documents.Field contentField = doc.GetField("Content");
                        Lucene.Net.Documents.Field pathField = doc.GetField("Path");
                        if (!System.IO.File.Exists(pathField.StringValue)) //判断本地是否存在该文件，存在则在检索结果栏里显示出来
                        {
                            int docId = hit.Doc; //该文件的在索引里的文档号,Doc是该文档进入索引时Lucene的编号，默认按照顺序编的
                            reader.DeleteDocument(docId);//删除该索引
                            reader.Commit();
                            continue;
                        }
                        log.Debug(fileNameField.StringValue + " = " + pathField.StringValue);
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
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchButton_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchKey(new List<string>() { this.Keywords.Text });
        }
    }
}
