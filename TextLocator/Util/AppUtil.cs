using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Util
{
    /// <summary>
    /// 程序工具类
    /// </summary>
    public class AppUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 应用目录
        /// </summary>
        private static readonly string _AppDir = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 应用程序名称
        /// </summary>
        private static readonly string _AppName = Process.GetCurrentProcess().ProcessName.Replace(".exe", "");
        /// <summary>
        /// App.ini路径：_AppDir\\_AppName.ini
        /// </summary>
        private static readonly string _AppIniFile = _AppDir + "\\" + _AppName + ".ini";

        static AppUtil()
        {
            log.Info("当前App的ini文件路径为：" + _AppIniFile);
            // ini文件初始化
            try
            {
                DirectoryInfo dir = new DirectoryInfo(_AppIniFile.Substring(0, _AppIniFile.LastIndexOf("\\")));
                if (!dir.Exists)
                {
                    dir.Create();
                }
                FileInfo file = new FileInfo(_AppIniFile);
                if (!file.Exists)
                {
                    file.Create();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        #region {AppName}.ini文件

        /// <summary>
        /// 设置INI
        /// </summary>
        /// <param name="section">缓冲区</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void WriteIni(string section, string key, string value)
        {
            try
            {
                WritePrivateProfileString(section, key, value, _AppIniFile);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        /// <summary>
        /// 读取INI
        /// </summary>
        /// <param name="section">缓冲区</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static string ReadIni(string section, string key, string def = "")
        {
            try
            {
                StringBuilder temp = new StringBuilder(255);
                int i = GetPrivateProfileString(section, key, def, temp, 255, _AppIniFile);

                return temp.ToString();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return def;
            }
        }
        /// <summary>
        /// Win32API：写入配置
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32")] //返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        /// <summary>
        /// Win32API：读取配置
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="retVal"></param>
        /// <param name="size"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32")] //返回取得字符串缓冲区的长度
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion
    }
}
