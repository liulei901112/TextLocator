using log4net;
using System;
using System.IO;
using System.Text;
using TextLocator.Core;
using TextLocator.Factory;
using TextLocator.Util;

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
                using (FileStream fs = File.OpenRead(filePath))
                {
                    using (StreamReader reader = new StreamReader(fs, FileUtil.GetEncoding(filePath)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            builder.AppendLine(AppConst.REGEX_TAG.Replace(line, ""));
                        }
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
