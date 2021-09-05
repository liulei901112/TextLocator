using log4net;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TextLocator.Service
{
    /// <summary>
    /// Xml文件文服务
    /// </summary>
    public class XmlFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetFileContent(string filePath)
        {
            // 文件内容
            string content = "";
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8))
                {
                    content = reader.ReadToEnd();

                    content = Regex.Replace(content, "\\<.[^<>]*\\>", "");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            return content;
        }
    }
}
