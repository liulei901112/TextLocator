using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using TextLocator.Consts;
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
                case FileType.Word类型:
                    bitmap = Properties.Resources.word;
                    break;
                case FileType.Excel类型:
                    bitmap = Properties.Resources.excel;
                    break;
                case FileType.PowerPoint类型:
                    bitmap = Properties.Resources.ppt;
                    break;
                case FileType.PDF类型:
                    bitmap = Properties.Resources.pdf;
                    break;
                default:
                    bitmap = Properties.Resources.txt;
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
            bitmap.Dispose();
            return bi;
        }

        /// <summary>
        /// 获取全部文件
        /// </summary>
        /// <param name="rootPath">根目录路径</param>
        /// <returns></returns>
        public static List<string> GetAllFiles(string rootPath)
        {
            log.Debug("根目录：" + rootPath);
            // 声明一个files包，用来存储遍历出的word文档
            List<string> filePaths = new List<string>();
            // 获取全部文件列表
            GetAllFiles(rootPath, filePaths);

            // 返回文件列表
            return filePaths;
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
            /*if (fileSize <= 0)
            {
                return false;
            }
            return fileSize / 1024 / 1024 > range;*/
        }

        /// <summary>
        /// 获取指定根目录下的子目录及其文档
        /// </summary>
        /// <param name="rootPath">根目录路径</param>
        /// <param name="filePaths">文档列表</param>
        private static void GetAllFiles(string rootPath, List<string> filePaths)
        {
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            // 得到所有子目录
            try
            {
                string[] dirs = Directory.GetDirectories(rootPath);
                foreach (string di in dirs)
                {
                    // 递归调用
                    GetAllFiles(di, filePaths);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }

            // 文件类型过滤
            string fileExtFilter = AppConst.FILE_EXTENSIONS.Replace(",", "|");

            try
            {
                string regex = @"^.+\.(" + fileExtFilter +  ")$";

                // 查找word文件
                string[] paths = Directory.GetFiles(dir.FullName)
                    .Where(file => Regex.IsMatch(file, regex))
                    .ToArray();
                // 遍历每个文档
                foreach (string path in paths)
                {
                    filePaths.Add(path);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }
    }
}
