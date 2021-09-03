using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TextLocator.Enums;
using TextLocator.Factory;
using TextLocator.Service;

namespace TextLocator
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Word服务
            FileInfoServiceFactory.Register(FileType.Word类型, new WordFileService());
            // Excel服务
            FileInfoServiceFactory.Register(FileType.Excel类型, new ExcelFileService());
            // PowerPoint服务
            FileInfoServiceFactory.Register(FileType.PowerPoint类型, new PowerPointFileService());
            // PDF服务
            FileInfoServiceFactory.Register(FileType.PDF类型, new PdfFileService());
            // HTML或XML服务
            FileInfoServiceFactory.Register(FileType.HTML或XML类型, new XmlFileService());
            // 纯文本服务
            FileInfoServiceFactory.Register(FileType.纯文本, new TxtFileService());
            // 其他类型服务
            FileInfoServiceFactory.Register(FileType.其他类型, new OtherFileService());
        }
    }
}
