using log4net;
using System;
using System.IO;
using System.Text;

namespace TextLocator.Service
{
    /// <summary>
    /// 文本文件服务
    /// </summary>
    public class TxtFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static volatile object locker = new object();

        public string GetFileContent(string filePath)
        {
            // 文件内容
            string content = "";
            lock (locker)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                    {
                        StringBuilder builder = new StringBuilder();
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            builder.Append(line);
                        }
                        reader.Close();
                        reader.Dispose();

                        content = builder.ToString();
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
