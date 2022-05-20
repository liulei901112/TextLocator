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
            Bitmap bitmap;
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
                case FileType.常用图片:
                    bitmap = Properties.Resources.image;
                    break;
                case FileType.程序员代码:
                    bitmap = Properties.Resources.code;
                    break;
                case FileType.TXT文档:
                    bitmap = Properties.Resources.txt;
                    break;
                case FileType.常用压缩包:
                    bitmap = Properties.Resources.zip;
                    break;
                default:
                    bitmap = Properties.Resources.none;
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
        /// 获取指定根目录下的子目录及其文档
        /// </summary>
        /// <param name="filePaths">文档列表</param>
        /// <param name="rootPath">根目录路径</param>
        /// <param name="fileExtRegex">文件后缀正则</param>
        public static void GetAllFiles(List<string> filePaths, string rootPath, Regex fileExtRegex)
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
                    if (AppConst.REGEX_EXCLUDE_KEYWORD.IsMatch(dirPath.ToUpper()))
                    {
                        continue;
                    }
                    // 递归调用
                    GetAllFiles(filePaths, dirPath, fileExtRegex);
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
                    .Where(file => fileExtRegex.IsMatch(file))
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
                log.Error(ex.Message, ex);
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
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(srcDir);
                dirInfo.Delete(true);
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

        /// <summary>
        /// C#根据字节数据byte[]前2位判断文本文件的Encoding编码格式
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(byte[] bs)
        {
            Encoding result = Encoding.Default;

            using (MemoryStream ms = new MemoryStream(bs))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte[] buffer = br.ReadBytes(2);

                    if (buffer[0] >= 0xEF)
                    {
                        if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                        {
                            result = Encoding.UTF8;
                        }
                        else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                        {
                            result = Encoding.BigEndianUnicode;
                        }
                        else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                        {
                            result = Encoding.Unicode;
                        }
                        else
                        {
                            result = Encoding.Default;
                        }
                    }
                    else
                    {
                        result = Encoding.Default;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取文件编码格式
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static Encoding GetEncoding(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
                byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
                byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
                Encoding reVal = Encoding.Default;
                using (BinaryReader br = new BinaryReader(fs, Encoding.Default))
                {
                    int.TryParse(fs.Length.ToString(), out int i);
                    byte[] ss = br.ReadBytes(i);
                    if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
                    {
                        reVal = Encoding.UTF8;
                    }
                    else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                    {
                        reVal = Encoding.BigEndianUnicode;
                    }
                    else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
                    {
                        reVal = Encoding.Unicode;
                    }
                    br.Dispose();
                }
                return reVal;
            }
        }

        /// <summary>
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static bool IsUTF8Bytes(byte[] data)
        {
            // 计算当前正分析的字符应还有的字节数
            int charByteCounter = 1;
            // 当前分析的字节
            byte curByte;
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        // 判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        // 标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // 若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
    }
}
