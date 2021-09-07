using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Service
{
    /// <summary>
    /// 图片文件服务
    /// </summary>
    public class ImageFileService : IFileInfoService
    {
        public string GetFileContent(string filePath)
        {
            return filePath;
        }
    }
}
