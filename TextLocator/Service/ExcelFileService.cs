using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TextLocator.Core;
using Excel = NetOffice.ExcelApi;

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
                    // 1、=========== DocumentFormat.OpenXml ===========
                    content = OpenXmlParse(filePath); 
                }
                catch (Exception ex)
                {
                   log.Error(filePath + " -> DocumentFormat.OpenXml 无法解析：" + ex.Message, ex);
                    try
                    {
                        // 2、=========== NPOI ===========
                        content = NPOIParse(filePath);
                    }
                    catch (Exception ex1)
                    {
                        log.Error(filePath + " -> NPOI 无法解析：" + ex1.Message, ex1);
                        try
                        {
                            // 3、=========== NetOffice.ExcelApi ===========                           
                            content = NetOfficeParse(filePath);
                        }
                        catch (Exception ex2)
                        {
                            log.Error(filePath + " -> NetOffice.ExcelApi 无法解析：" + ex2.Message, ex2);
                            try
                            {
                                // 4、=========== Spire.XLS ===========
                                content = SpireXlsParse(filePath);
                            }
                            catch (Exception ex3)
                            {
                                log.Error(filePath + " -> Spire.XLS 无法解析：" + ex3.Message, ex3);
                            }
                        }
                    }
                }
            }
            return content;
        }

        /// <summary>
        /// NetOffice 解析
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string NetOfficeParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using(Excel.Application app = new Excel.Application())
            {
                try
                {
                    // 参数（缺省）
                    object missing = Missing.Value;

                    using (Excel.Workbook workbook = app.Workbooks.Open(filePath, false, true))
                    {
                        // 遍历工作簿
                        foreach(Excel.Worksheet sheet in workbook.Worksheets)
                        {
                            // 遍历行
                            int rowCount = sheet.UsedRange.Cells.Rows.Count;
                            int columnCount = sheet.UsedRange.Cells.Columns.Count;
                            for(int i = 0; i < rowCount; i++)
                            {
                                for (int j = 0; j < columnCount; j++)
                                {
                                    builder.Append(sheet.Cells[i + 1, j + 1].Value + "　");
                                }
                                builder.AppendLine();
                            }
                            builder.AppendLine();
                        }
                    }
                }
                finally
                {
                    try
                    {
                        app.Quit();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Excel关闭错误：" + ex.Message, ex);
                    }

                    AppCore.ManualGC();
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// DocumentFormat.OpenXml 解析
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string OpenXmlParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(fs, false))
                {
                    WorkbookPart workbook = document.WorkbookPart;
                    // 获取所有工作薄
                    IEnumerable<Sheet> sheets = workbook.Workbook.Descendants<Sheet>();
                    if (!sheets.Any())
                    {
                        throw new ArgumentException("空的Excel文档");
                    }
                    // 遍历工作簿
                    sheets.ToList().ForEach(sheet => {
                        // 工作表
                        WorksheetPart worksheet = (WorksheetPart)document.WorkbookPart.GetPartById(sheet.Id);
                        // 获取数据行
                        var rows = worksheet.Worksheet.Descendants<Row>();
                        foreach (Row r in rows)
                        {
                            foreach (Cell c in r.Elements<Cell>())
                            {
                                builder.Append(GetCellValue(c, workbook, 2) + "　");
                            }
                            builder.AppendLine();
                        }
                    });
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// NPOI 解析
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string NPOIParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
                IWorkbook workbook = null;
                string extName = Path.GetExtension(filePath);
                if (".xls".Equals(extName))
                {
                    workbook = new HSSFWorkbook(fs);
                }
                else
                {
                    workbook = new XSSFWorkbook(fs);
                }

                // 遍历表格
                int sheetCount = workbook.NumberOfSheets;
                for (int i = 0; i < sheetCount; i++)
                {
                    // 读取当前表格
                    ISheet sheet = workbook.GetSheetAt(i);

                    int rowCount = sheet.LastRowNum;
                    for (int j = 0; j <= rowCount; j++)
                    {
                        // 读取当前行
                        IRow row = sheet.GetRow(j);
                        if (row != null)
                        {
                            int cellCount = row.LastCellNum;
                            for (int k = 0; k < cellCount; k++)
                            {
                                builder.Append(row.GetCell(k) + "　");
                            }
                            builder.AppendLine();
                        }
                    }
                    builder.AppendLine();
                }
                if (workbook != null)
                {
                    workbook.Close();
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Spire.XLS
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private string SpireXlsParse(string filePath)
        {
            StringBuilder builder = new StringBuilder();
            using (FileStream fs = File.OpenRead(filePath))
            {
                // 创建Workbook对象
                using (Spire.Xls.Workbook workbook = new Spire.Xls.Workbook())
                {
                    // 加载Excel文档
                    workbook.LoadFromStream(fs);
                    if (workbook != null)
                    {
                        Spire.Xls.Collections.WorksheetsCollection sheets = workbook.Worksheets;
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
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 获取单位格的值
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="workbookPart"></param>
        /// <param name="type">1 不去空格 2 前后空格 3 所有空格  </param>
        /// <returns></returns>
        private string GetCellValue(Cell cell, WorkbookPart workbook, int type = 2)
        {
            // 合并单元格不做处理
            if (cell.CellValue == null)
                return string.Empty;

            string cellInnerText = cell.CellValue.InnerXml;

            // 纯字符串
            if (cell.DataType != null && (cell.DataType.Value == CellValues.SharedString || cell.DataType.Value == CellValues.String || cell.DataType.Value == CellValues.Number))
            {
                //获取spreadsheetDocument中共享的数据
                SharedStringTable stringTable = workbook.SharedStringTablePart.SharedStringTable;

                // 如果共享字符串表丢失，则说明出了问题。
                if (!stringTable.Any())
                    return string.Empty;

                string text = stringTable.ElementAt(int.Parse(cellInnerText)).InnerText;
                if (type == 2)
                    return text.Trim();
                else if (type == 3)
                    return text.Replace(" ", "");
                else
                    return text;
            }
            // bool类型
            else if (cell.DataType != null && cell.DataType.Value == CellValues.Boolean)
            {
                return (cellInnerText != "0").ToString().ToUpper();
            }
            // 数字格式代码（numFmtId）小于164是内置的：https://www.it1352.com/736329.html
            else
            {
                // 为空为数值
                if (cell.StyleIndex == null)
                    return cellInnerText;

                Stylesheet styleSheet = workbook.WorkbookStylesPart.Stylesheet;
                CellFormat cellFormat = (CellFormat)styleSheet.CellFormats.ChildElements[(int)cell.StyleIndex.Value];

                uint formatId = cellFormat.NumberFormatId.Value;
                // OLE 自动化日期值
                double doubleTime;
                // yyyy/MM/dd HH:mm:ss
                DateTime dateTime;
                switch (formatId)
                {
                    // 常规
                    case 0:
                        return cellInnerText;
                    // 百分比【0%】
                    case 9:
                    // 百分比【0.00%】
                    case 10:
                    // 科学计数【1.00E+02】
                    case 11:
                    // 分数【1/2】
                    case 12:
                        return cellInnerText;
                    case 14:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("yyyy/MM/dd");
                    // case 15:
                    // case 16:
                    case 17:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("yyyy/MM");
                    // case 18:
                    // case 19:
                    case 20:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("H:mm");
                    case 21:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("HH:mm:ss");
                    case 22:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("yyyy/MM/dd HH:mm");
                    // case 45:
                    // case 46:
                    case 47:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("yyyy/MM/dd");
                    // 【中国】11月11日
                    case 58:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("MM/dd");
                    // 【中国】2020年11月11日
                    case 176:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("yyyy/MM/dd");
                    // 【中国】11:22:00
                    case 177:
                        doubleTime = double.Parse(cellInnerText);
                        dateTime = DateTime.FromOADate(doubleTime);
                        return dateTime.ToString("HH:mm:ss");
                    default:
                        return cellInnerText;
                }
            }
        }
    }
}
