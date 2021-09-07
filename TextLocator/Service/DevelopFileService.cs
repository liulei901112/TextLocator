using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextLocator.Service
{
    /// <summary>
    /// 开发者文件服务
    /// </summary>
    public class DevelopFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static volatile object locker = new object();

        public string GetFileContent(string filePath)
        {
            // 获取扩展名
            string extName = Path.GetExtension(filePath);
            // 文件内容
            string content = "";
            lock (locker)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8))
                    {
                        content = reader.ReadToEnd();

                        content = Regex.Replace(content, "\\<.[^<>]*\\>", "");

                        reader.Close();
                        reader.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
            return content;
        }
    }
}
