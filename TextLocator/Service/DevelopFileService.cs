using log4net;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TextLocator.Core;

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
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8))
                {
                    StringBuilder builder = new StringBuilder();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        builder.Append(line);
                    }

                    content = AppConst.REGIX_TAG.Replace(builder.ToString(), "");

                    reader.Close();
                    reader.Dispose();
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
