using log4net;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TextLocator.Core;

namespace TextLocator.Service
{
    /// <summary>
    /// Dom文件文服务
    /// </summary>
    public class DomFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static object locker = new object();

        public string GetFileContent(string filePath)
        {
            // 文件内容
            StringBuilder builder = new StringBuilder();
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read), Encoding.UTF8))
                {
                    string line;
                    while((line = reader.ReadLine()) != null)
                    {
                        builder.Append(AppConst.REGEX_TAG.Replace(line, ""));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(filePath + " -> " + ex.Message, ex);
            }
            return builder.ToString(); ;
        }
    }
}
