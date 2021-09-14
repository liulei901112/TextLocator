using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        
        /// <summary>
        /// Ini文件内容缓存
        /// </summary>
        private static readonly Dictionary<string, string> _AppIniCache = new Dictionary<string, string>();

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

            // 加载节点下全部Key-Value
            LoadAllKeyValue("FileIndex", _AppIniCache);
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
                _AppIniCache[GetCacheKey(section, key)] = value;
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
                if (_AppIniCache.ContainsKey(GetCacheKey(section, key)))
                {
                    return _AppIniCache[GetCacheKey(section, key)];
                }
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
        /// 加载节点下全部KeyValue
        /// </summary>
        /// <param name="section">节点</param>
        /// <returns></returns>
        private static void LoadAllKeyValue(string section, Dictionary<string, string> cacheDic)
        {
            Thread t = new Thread(() =>
            {
                byte[] buffer = new byte[512000000];
                GetPrivateProfileSection(section, buffer, buffer.Length, _AppIniFile);
                string[] tmp = Encoding.Default.GetString(buffer).Trim('\0').Split('\0');
                foreach (string entry in tmp)
                {
                    string[] v = entry.Split('=');

                    cacheDic[GetCacheKey(section, v[0])] = v[1];
                }
                log.Debug("加载" + section + "节点下全部键值，总数：" + cacheDic.Count);
            });
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }

        /// <summary>
        /// 获取缓存Key（节点名称 + Key）
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetCacheKey(string section, string key)
        {
            return section + "_" + key;
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

        /// <summary>
        /// Win32API：读取指定节点下全部键值
        /// </summary>
        /// <param name="lpAppName"></param>
        /// <param name="lpszReturnBuffer"></param>
        /// <param name="nSize"></param>
        /// <param name="lpFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);
        #endregion
    }
}
