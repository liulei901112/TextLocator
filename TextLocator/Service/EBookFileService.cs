using Common.Logging;
using Roler.Toolkit.File.Mobi;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Service
{
    /// <summary>
    /// 电子书文件服务
    /// </summary>
    internal class EBookFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetFileContent(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            string content = string.Empty;
            switch(extension)
            {
                case ".mobi":
                    content = GetMobiFileContent(filePath);
                    break;
            }
            return content;
        }

        /// <summary>
        /// mobi后缀
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetMobiFileContent(string filePath)
        {
            // 内容
            StringBuilder builder = new StringBuilder();
            using (Stream fs = new FileStream(filePath, FileMode.OpenOrCreate)) {
                Roler.Toolkit.File.Mobi.MobiReader mobiReader = new Roler.Toolkit.File.Mobi.MobiReader(fs);
                Mobi mobi = mobiReader.Read();

                builder.Append("作者：" + mobi.Creator);
                builder.Append("描述：" + mobi.Description);
                builder.Append("出版商：" + mobi.Publisher);

                Structure structure = mobi.Structure;

                builder.Append("\r\n");
                builder.Append(mobi.Text);
            }
            return builder.ToString();
        }
    }
}
