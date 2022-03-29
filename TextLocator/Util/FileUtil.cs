using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TextLocator.Core;
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
        /// 文件大小单位
        /// </summary>
        private static readonly string[] suffixes = new string[] { " B", " KB", " MB", " GB", " TB", " PB" };
        /// <summary>
        /// 根据文件类型获取文件图标
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static BitmapImage GetFileIcon(FileType fileType)
        {
            Bitmap bitmap = null;
            switch (fileType)
            {
                case FileType.Word文档:
                    bitmap = Properties.Resources.word;
                    break;
                case FileType.Excel表格:
                    bitmap = Properties.Resources.excel;
                    break;
                case FileType.PPT文稿:
                    bitmap = Properties.Resources.ppt;
                    break;
                case FileType.PDF文档:
                    bitmap = Properties.Resources.pdf;
                    break;
                case FileType.DOM文档:
                    bitmap = Properties.Resources.html;
                    break;
                case FileType.图片:
                    bitmap = Properties.Resources.image;
                    break;
                case FileType.代码:
                    bitmap = Properties.Resources.code;
                    break;
                case FileType.纯文本:
                    bitmap = Properties.Resources.txt;
                    break;
                case FileType.压缩包:
                    bitmap = Properties.Resources.zip;
                    break;
                default:
                    bitmap = Properties.Resources.rtf;
                    break;
            }
            BitmapImage bi = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, bitmap.RawFormat);
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
            }

            try
            {
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            return bi;
        }

        /// <summary>
        /// 获取文件大小友好显示
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetFileSizeFriendly(long number)
        {
            double last = 1;
            for (int i = 0; i < suffixes.Length; i++)
            {
                var current = Math.Pow(1024, i + 1);
                var temp = number / current;
                if (temp < 1)
                {
                    return (number / last).ToString("n2") + suffixes[i];
                }
                last = current;
            }
            return number.ToString();
        }

        /// <summary>
        /// 超出范围
        /// </summary>
        /// <param name="fileSize"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool OutOfRange(long fileSize, int range = 10)
        {
            if (fileSize > range) { }
            return false;
        }

        /// <summary>
        /// 获取指定根目录下的子目录及其文档
        /// </summary>
        /// <param name="filePaths">文档列表</param>
        /// <param name="rootPath">根目录路径</param>
        /// <param name="regexExclude">过滤列表</param>
        public static void GetAllFiles(List<string> filePaths, string rootPath, Regex regexExclude = null)
        {
            // 根目录
            DirectoryInfo rootDir = new DirectoryInfo(rootPath);
            // 文件夹处理
            try
            {
                var dirs = rootDir.EnumerateDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    // 判断权限
                    if ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }
                    string dirPath = dir.FullName;
                    // 系统过滤：$RECYCLE|360REC|SYSTEM|TEMP|SYSTEM VOLUME INFOMATION
                    // 自定义过滤：
                    if (AppConst.REGEX_EXCLUDE_KEYWORD.IsMatch(dirPath.ToUpper()) || (regexExclude != null && regexExclude.IsMatch(dirPath)))
                    {
                        continue;
                    }
                    // 递归调用
                    GetAllFiles(filePaths, dirPath, regexExclude);
                }
            }
            catch (UnauthorizedAccessException ex) {
                log.Warn(ex.Message, ex);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }

            // 文件处理
            try
            {
                // 查找word文件
                string[] paths = Directory.GetFiles(rootPath)
                    .Where(file => AppConst.REGEX_FILE_EXT.IsMatch(file))
                    .ToArray();
                // 遍历每个文档
                foreach (string path in paths)
                {
                    string fileName = path.Substring(path.LastIndexOf("\\") + 1);
                    //if (fileName.StartsWith("`") || fileName.StartsWith("$") || fileName.StartsWith("~") || fileName.StartsWith("."))
                    if (AppConst.REGEX_START_WITH.IsMatch(fileName))
                    {
                        continue;
                    }
                    filePaths.Add(path);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                log.Warn(ex.Message, ex);
            }
            catch (Exception ex) {
                log.Warn(ex.Message, ex);
            }
        }

        /// <summary>
        /// 删除目录下全部文件
        /// </summary>
        /// <param name="srcDir"></param>
        public static void RemoveDirectory(string srcDir)
        {
            // 若目标文件夹不存在
            if (!Directory.Exists(srcDir))
            {
                return;
            }
            // 获取源文件夹中的所有文件完整路径
            FileInfo[] files = new DirectoryInfo(srcDir).GetFiles();
            foreach (FileInfo fileInfo in files)
            {
                try
                {
                    File.Delete(fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    log.Error(fileInfo.FullName + " -> 文件删除失败：" + ex.Message, ex);
                }
            }
            try
            {
                Directory.Delete(srcDir);
            } catch (Exception ex) {
                log.Error(srcDir + " -> 目录删除失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 拷贝源目录到新目录
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="destDir"></param>
        public static void CopyDirectory(string srcDir, string destDir)
        {
            // 若目标文件夹不存在
            if (!Directory.Exists(destDir))
            {
                // 创建目标文件夹
                Directory.CreateDirectory(destDir);
            }
            string newPath;

            // 获取源文件夹中的所有文件完整路径
            FileInfo[] files = new DirectoryInfo(srcDir).GetFiles();
            // 遍历文件
            foreach (FileInfo fileInfo in files)
            {
                newPath = destDir + "\\" + fileInfo.Name;
                try
                {
                    File.Copy(fileInfo.FullName, newPath, true);
                }
                catch (Exception ex)
                {
                    log.Error("索引拷贝错误：" + ex.Message, ex);
                }
            }
            string[] dirs = Directory.GetDirectories(srcDir);
            // 遍历文件夹
            foreach (string path in dirs)
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                string newDir = destDir + directory.Name;
                CopyDirectory(path + "\\", newDir + "\\");
            }
        }
    }
}
