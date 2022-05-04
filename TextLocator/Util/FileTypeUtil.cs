using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            FileType fileType = FileType.TXT文档;
            
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
                foreach(var ext in description.Split(','))
                {
                    if (ext.Equals(fileExt.Replace(".", ""))) {
                        fileType = ft;
                        break;
                    }
                }
            }

            return fileType;
        }

        /// <summary>
        /// 文件类型后缀列表（ext1,ext2...）
        /// </summary>
        /// <param name="fileTypes">文件类型列表</param>
        /// <param name="separator">分隔符，默认【，】</param>
        /// <returns></returns>
        public static string ConvertToFileTypeExts(List<FileType> fileTypes, string separator = ",")
        {
            string exts = "";
            // 遍历文件类型，根据后缀查找文件类型
            foreach (FileType ft in fileTypes)
            {
                string description = ft.GetDescription();
                if (!string.IsNullOrEmpty(description)) {
                    exts += description + separator;
                }
            }
            return exts.Substring(0, exts.Length - 1).Replace(",", separator);
        }

        /// <summary>
        /// 获取不包含全部的文件类型列表
        /// </summary>
        /// <returns></returns>
        public static List<FileType> GetFileTypesNotAll()
        {
            List<FileType> fileTypes = new List<FileType>();
            foreach(FileType ft in Enum.GetValues(typeof(FileType)))
            {
                if (ft == FileType.全部)
                {
                    continue;
                }
                fileTypes.Add(ft);
            }
            return fileTypes;
        }
    }
}
