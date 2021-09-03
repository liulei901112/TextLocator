using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using TextLocator.Enums;

namespace TextLocator.Util
{
    /// <summary>
    /// 文件工具类
    /// </summary>
    public class FileUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 图标集合
        /// </summary>
        private static readonly Dictionary<string, BitmapImage> icons = new Dictionary<string, BitmapImage>();

        static FileUtil()
        {
            icons.Add("word", new BitmapImage(new Uri(@"/Resource/ext/word.png", UriKind.Relative)));
            icons.Add("excel", new BitmapImage(new Uri(@"/Resource/ext/excel.png", UriKind.Relative)));
            icons.Add("ppt", new BitmapImage(new Uri(@"/Resource/ext/rtf.png", UriKind.Relative)));

            icons.Add("pdf", new BitmapImage(new Uri(@"/Resource/ext/pdf.png", UriKind.Relative)));

            icons.Add("txt", new BitmapImage(new Uri(@"/Resource/ext/txt.png", UriKind.Relative)));
            icons.Add("html", new BitmapImage(new Uri(@"/Resource/ext/html.png", UriKind.Relative)));

            icons.Add("eml", new BitmapImage(new Uri(@"/Resource/ext/eml.png", UriKind.Relative)));
            icons.Add("rtf", new BitmapImage(new Uri(@"/Resource/ext/rtf.png", UriKind.Relative)));
        }

        /// <summary>
        /// 获取全部文件
        /// </summary>
        /// <param name="rootPath">根目录路径</param>
        /// <returns></returns>
        public static List<FileInfo> GetAllFiles(string rootPath)
        {
            log.Debug("根目录：" + rootPath);
            // 声明一个files包，用来存储遍历出的word文档
            List<FileInfo> files = new List<FileInfo>();
            // 获取全部文件列表
            GetAllFiles(rootPath, files);

            // 返回文件列表
            return files;
        }

        /// <summary>
        /// 获取指定根目录下的子目录及其文档
        /// </summary>
        /// <param name="rootPath">根目录路径</param>
        /// <param name="files">word文档存储包</param>
        private static void GetAllFiles(string rootPath, List<FileInfo> files)
        {
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            // 得到所有子目录
            try
            {
                string[] dirs = Directory.GetDirectories(rootPath);
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
            FileInfo[] fis = dir.GetFiles("*.doc?");
            // 遍历每个word文档
            foreach (FileInfo fi in fis)
            {
                files.Add(fi);
            }
        }

        /// <summary>
        /// 根据文件类型获取图标
        /// </summary>
        /// <param name="fileType"></param>
        public static BitmapImage GetFileTypeIcon(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Word类型:
                    return icons["word"];
                case FileType.Excel类型:
                    return icons["excel"];
                case FileType.PowerPoint类型:
                    return icons["ppt"];
                case FileType.PDF类型:
                    return icons["pdf"];
                case FileType.HTML或XML类型:
                    return icons["html"];
                case FileType.纯文本:
                    return icons["txt"];
                case FileType.其他类型:
                    return icons["txt"];
            }
            return null;
        }
    }
}
