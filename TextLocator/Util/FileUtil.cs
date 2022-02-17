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
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static string GetFileSizeFriendly(long fileSize)
        {
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
            return fileSize + "" + fileSizeUnit;
        }

        /// <summary>
        /// 超出范围
        /// </summary>
        /// <param name="fileSize"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool OutOfRange(long fileSize, int range = 10)
        {
            return false;
        }

        /// <summary>
        /// 获取指定根目录下的子目录及其文档
        /// </summary>
        /// <param name="rootPath">根目录路径</param>
        /// <param name="filePaths">文档列表</param>
        public static void GetAllFiles(string rootPath, List<string> filePaths)
        {
            // 根目录
            DirectoryInfo rootDir = new DirectoryInfo(rootPath);

            // 文件夹处理
            try
            {
                string[] dirs = Directory.GetDirectories(rootPath);
                foreach (string dir in dirs)
                {
                    string du = dir.ToUpper();
                    // $开始、360REC开头、SYSTEM、TEMP
                    if (du.Contains("$RECYCLE") || du.Contains("360REC") || du.Contains("SYSTEM") || du.Contains("TEMP"))
                    {
                        continue;
                    }
                    // 递归调用
                    GetAllFiles(dir, filePaths);
                }
            }
            catch { }

            // 文件处理
            try
            {
                // 查找word文件
                string[] paths = Directory.GetFiles(rootDir.FullName)
                    .Where(file => AppConst.REGIX_FILE_EXT.IsMatch(file))
                    .ToArray();
                // 遍历每个文档
                foreach (string path in paths)
                {
                    string fileName = path.Substring(path.LastIndexOf("\\") + 1);
                    if (fileName.StartsWith("`") || fileName.StartsWith("$") || fileName.StartsWith("~") || fileName.StartsWith("."))
                    {
                        continue;
                    }
                    filePaths.Add(path);
                }
            }
            catch { }
        }

        /// <summary>
        /// 删除目录下全部文件
        /// </summary>
        /// <param name="aPP_INDEX_DIR"></param>
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
                    log.Error("索引清理失败：" + ex.Message, ex);
                }
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
