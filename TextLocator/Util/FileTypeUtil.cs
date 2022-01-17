using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextLocator.Enums;

namespace TextLocator.Util
{
    /// <summary>
    /// 文件类型工具类
    /// </summary>
    public class FileTypeUtil
    {

        /// <summary>
        /// 根据路径返回文件类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static FileType GetFileType(string filePath)
        {
            // 默认为其他类型文件
            FileType fileType = FileType.纯文本;
            
            // 判断文件路径
            if (string.IsNullOrEmpty(filePath))
            {
                return fileType;
            }

            // 获取扩展名
            string fileExt = Path.GetExtension(filePath);

            // 验证文件后缀是不是空，或者默认值
            if (string.IsNullOrEmpty(fileExt) || fileExt.Equals("default")) 
            {
                return fileType;
            }

            // 遍历文件类型，根据后缀查找文件类型
            foreach (FileType ft in Enum.GetValues(typeof(FileType)))
            {
                // 获取描述
                string description = ft.GetDescription();
                if (description.Contains(fileExt.Replace(".", "")))
                {
                    fileType = ft;
                    break;
                }
            }

            return fileType;
        }

        /// <summary>
        /// 文件类型后缀列表（ext1,ext2...）
        /// </summary>
        /// <param name="separator">分隔符，默认【，】</param>
        /// <returns></returns>
        public static string GetFileTypeExts(string separator = ",")
        {
            string fileTypeExts = "";
            // 遍历文件类型，根据后缀查找文件类型
            foreach (FileType ft in Enum.GetValues(typeof(FileType)))
            {
                // 获取描述
                string description = ft.GetDescription();
                if (!string.IsNullOrEmpty(description)) {
                    fileTypeExts += description + separator;
                }
            }
            return fileTypeExts.Substring(0, fileTypeExts.Length - 1).Replace(",", separator);
        }
    }
}
