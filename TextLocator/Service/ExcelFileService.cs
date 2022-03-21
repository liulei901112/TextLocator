using log4net;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Spire.Xls.Collections;
using System;
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

        private static volatile object locker = new object();

        public string GetFileContent(string filePath)
        {
            // 文件内容
            string content = "";
            lock (locker)
            {
                try
                {
                    // =========== Spire.XLS ===========
                    // 创建Workbook对象
                    using (Spire.Xls.Workbook workbook = new Spire.Xls.Workbook())
                    {
                        // 加载Excel文档
                        workbook.LoadFromFile(filePath);
                        StringBuilder builder = new StringBuilder();
                        if (workbook != null)
                        {
                            WorksheetsCollection sheets = workbook.Worksheets;
                            if (sheets != null && sheets.Count > 0)
                            {
                                // 获取工作表
                                for (int i = 0; i < sheets.Count; i++)
                                {
                                    using (Spire.Xls.Worksheet sheet = sheets[i])
                                    {
                                        // 行
                                        for (int j = sheet.FirstRow; j < sheet.LastRow; j++)
                                        {
                                            using (Spire.Xls.CellRange row = sheet.Rows[j])
                                            {
                                                // 列
                                                for (int k = 0; k < row.Columns.Length; k++)
                                                {
                                                    builder.Append(row.Columns[k].Value2.ToString() + "　");
                                                }
                                            }
                                            builder.AppendLine();
                                        }
                                    }
                                }
                            }
                        }
                        content = builder.ToString();
                    }
                }
                catch (Exception spirex)
                {
                    log.Error(filePath + " -> " + spirex.Message + " Spire.Xls解析错误，尝试NPIO", spirex);
                    // =========== NPIO ===========
                    try
                    {
                        // 文件流
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            // 获取扩展名
                            string extName = Path.GetExtension(filePath);
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

                            if (readWorkbook != null)
                            {
                                StringBuilder builder = new StringBuilder();
                                // 获取表
                                var sheetCount = readWorkbook.NumberOfSheets;
                                if (sheetCount > 0)
                                {
                                    for (int i = 0; i < sheetCount; i++)
                                    {
                                        // 得到sheet数据
                                        ISheet sheet = readWorkbook.GetSheetAt(i);
                                        if (sheet != null)
                                        {
                                            // 获取行数
                                            var rowCount = sheet.LastRowNum;
                                            // 解析行数据
                                            for (int j = 0; j <= rowCount; j++)
                                            {
                                                // 得到row数据
                                                IRow row = sheet.GetRow(j);
                                                if (row != null)
                                                {
                                                    // 解析列数据
                                                    for (int k = 0; k < row.LastCellNum; k++)
                                                    {
                                                        // 得到cell数据
                                                        ICell cell = row.GetCell(k);
                                                        // 获取某行某列对应的单元格数据  
                                                        builder.Append(cell + "　");
                                                    }
                                                    // 换行
                                                    builder.AppendLine();
                                                }
                                            }
                                        }
                                    }
                                }
                                readWorkbook.Close();

                                content = builder.ToString();
                            }
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        log.Error(filePath + " -> " + ex.Message, ex);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        log.Error(filePath + " -> " + ex.Message, ex);
                    }
                    catch (Exception ex)
                    {
                        log.Error(filePath + " -> " + ex.Message, ex);
                    }
                }
            }
            return content;
        }
    }
}
