using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TextLocator.Enum;
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
            FileInfoServiceFactory.Register(FileType.Word类型, new WordService());
        }
    }
}
