using log4net;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
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
                try
                {
                    // =========== NPIO ===========

                    // 获取扩展名
                    string extName = Path.GetExtension(filePath);
                    // 文件流
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        // 读取IWorkbook
                        IWorkbook readWorkbook = null;
                        switch (extName)
                        {
                            // 把xls写入workbook中 2003版本
                            case ".xls":
                                readWorkbook = new HSSFWorkbook(fileStream);
                                break;
                            // 把xlsx 写入workbook中 2007版本
                            case ".xlsx":
                                readWorkbook = new XSSFWorkbook(fileStream);
                                break;
                            default:
                                break;
                        }
                        // 关闭文件流
                        fileStream.Close();

                        if (readWorkbook != null)
                        {
                            StringBuilder builder = new StringBuilder();
                            // 获取表
                            var sheetCount = readWorkbook.NumberOfSheets;
                            for (int i = 0; i < sheetCount; i++)
                            {
                                // 获取sheet表数据
                                ISheet sheet = readWorkbook.GetSheetAt(i);

                                // 获取行数
                                var rowCount = sheet.LastRowNum;

                                // 从第四行(下标为3)开始获取数据，前三行是表头
                                // 如果从第一行开始，则i=0就可以了
                                for (int j = 0; j <= rowCount; j++)
                                {
                                    // 获取具体行
                                    IRow row = sheet.GetRow(j);
                                    if (row != null)
                                    {
                                        // 获取行对应的列数
                                        for (int k = 0; k < row.LastCellNum; k++)
                                        {
                                            // 获取某行某列对应的单元格数据  
                                            builder.Append(row.GetCell(k) + ";");
                                        }
                                        // 换行
                                        builder.AppendLine();
                                    }
                                }
                                
                                readWorkbook.Close();
                            }

                            content = builder.ToString();
                        }
                    }
                }
                catch
                {
                    // =========== Spire.XLS ===========
                    // 创建Workbook对象
                    using (Spire.Xls.Workbook workbook = new Spire.Xls.Workbook())
                    {

                        // 加载Excel文档
                        workbook.LoadFromFile(filePath);

                        StringBuilder builder = new StringBuilder();

                        // 获取工作表
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            Spire.Xls.Worksheet sheet = workbook.Worksheets[i];

                            // 行
                            for (int j = sheet.FirstRow; j < sheet.LastRow; j++)
                            {
                                Spire.Xls.CellRange row = sheet.Rows[j];
                                // 列
                                for (int k = 0; k < row.Columns.Length; k++)
                                {
                                    builder.Append(row.Columns[k].Value2.ToString());
                                }
                                row.Dispose();
                                builder.AppendLine();
                            }
                            sheet.Dispose();
                        }

                        workbook.Dispose();

                        content = builder.ToString();
                    }
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
