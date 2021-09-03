using log4net;
using Spire.Xls;
using Spire.Xls.Core;
using System;
using System.Text;

namespace TextLocator.Service
{
    /// <summary>
    /// Excel文件服务
    /// </summary>
    public class ExcelFileService : IFileInfoService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetFileContent(string filePath)
        {
            // 文件内容
            string content = "";
            try
            {
                StringBuilder builder = new StringBuilder();

                Workbook workbook = new Workbook();
                workbook.LoadFromFile(filePath);
                for (int i = 0; i < workbook.Worksheets.Count; i++) {
                    Worksheet sheet = workbook.Worksheets[i];
                    for(int j = 0; j < sheet.PrstGeomShapes.Count; j++)
                    {
                        IPrstGeomShape shape = sheet.PrstGeomShapes[j];
                        builder.Append(shape.Text);
                    }
                }

                content = builder.ToString();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
            log.Debug(filePath + " => " + content);
            return content;
        }
    }
}
