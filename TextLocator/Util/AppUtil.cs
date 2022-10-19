using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TextLocator.Core;

namespace TextLocator.Util
{
    /// <summary>
    /// 程序工具类
    /// </summary>
    public class AppUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// 锁
        /// </summary>
        private static object locker = new object();

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
        private static readonly Dictionary<string, Dictionary<string, string>> _AppIniCache = new Dictionary<string, Dictionary<string, string>>();

        static AppUtil()
        {
            log.Info("当前App的ini文件路径为：" + _AppIniFile);

            // ini文件初始化
            Initialize();

            Thread t = new Thread(() =>
            {
                // 加载区域配置
                LoadAllKeyValue(AppConst.CacheKey.AREA_CONFIG_KEY);

                List<string> areaList = ReadSectionList(AppConst.CacheKey.AREA_CONFIG_KEY);
                if (areaList != null)
                {
                    foreach (string areaId in areaList)
                    {
                        LoadAllKeyValue(areaId);
                    }
                }
            });
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
            


            
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private static void Initialize()
        {
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

        #region 配置文件

        /// <summary>
        /// 设置INI
        /// </summary>
        /// <param name="section">缓冲区</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void WriteValue(string section, string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(section))
                {
                    throw new ArgumentException("必须指定节点名称", "section");
                }
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("必须指定键名称(key)", "key");
                }

                lock (locker)
                {
                    Dictionary<string, string> sectionDic = _AppIniCache.ContainsKey(section) ? _AppIniCache[section] : new Dictionary<string, string>();
                    if (sectionDic.ContainsKey(key))
                    {
                        if (string.IsNullOrEmpty(value))
                            sectionDic.Remove(key);
                        else
                            sectionDic[key] = value;
                    }
                    else
                    	sectionDic.Add(key, value);
                    if (_AppIniCache.ContainsKey(section))
                        _AppIniCache[section] = sectionDic;
                    else
                        _AppIniCache.Add(section, sectionDic);

                    WritePrivateProfileString(section, key, value, _AppIniFile);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("写入错误：section={0}，key={1}，value={2} => {3}", section, key, value, ex.Message), ex);
            }
        }

        /// <summary>
        /// 读取INI
        /// </summary>
        /// <param name="section">缓冲区</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static string ReadValue(string section, string key, string def = "")
        {
            try
            {
                const int SIZE = 1024 * 10;

                if (string.IsNullOrEmpty(section))
                {
                    throw new ArgumentException("必须指定节点名称", "section");
                }

                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("必须指定键名称(key)", "key");
                }
                lock (locker)
                {
                    Dictionary<string, string> sectionDic = _AppIniCache.ContainsKey(section) ? _AppIniCache[section] : new Dictionary<string, string>();
                    if (sectionDic.ContainsKey(key))
                    {
                        return sectionDic[key];
                    }
                }
                StringBuilder builder = new StringBuilder(SIZE);
                uint bytesReturned = GetPrivateProfileString(section, key, def, builder, SIZE, _AppIniFile);
                if (bytesReturned != 0)
                {
                    def = builder.ToString();
                    lock(locker)
                    {
                        Dictionary<string, string> sectionDic = _AppIniCache.ContainsKey(section) ? _AppIniCache[section] : new Dictionary<string, string>();
                        if (sectionDic.ContainsKey(key))
                            sectionDic[key] = def;
                        else
                            sectionDic.Add(key, def);

                        if (_AppIniCache.ContainsKey(section))
                            _AppIniCache[section] = sectionDic;
                        else
                            _AppIniCache.Add(section, sectionDic);
                    }                    
                }
                return def;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("读取错误：section={0}，key={1} => {2}", section, key, ex.Message), ex);
                return def;
            }
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="section">节点</param>
        /// <returns>操作是否成功</returns>
        public static void DeleteSection(string section)
        {
            try
            {
                if (string.IsNullOrEmpty(section))
                {
                    throw new ArgumentException("必须指定节点名称", "section");
                }
                lock (locker)
                {
                    if (_AppIniCache.ContainsKey(section))
                    {
                        _AppIniCache.Remove(section);
                    }
                }
                WritePrivateProfileString(section, null, null, _AppIniFile);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("删除错误：section={0}，key={1} => {2}", section, ex.Message), ex);
            }
        }

        public static List<string> ReadSectionList(string section)
        {
            try
            {
                if (string.IsNullOrEmpty(section))
                {
                    throw new ArgumentException("必须指定节点名称", "section");
                }
                lock (locker)
                {
                    if (_AppIniCache.ContainsKey(section))
                    {
                        return _AppIniCache[section].Keys.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("读取错误：section={0}，key={1} => {2}", section, ex.Message), ex);
            }
            return null;
        }

        /// <summary>
        /// 加载节点下全部KeyValue
        /// </summary>
        /// <param name="section">节点</param>
        /// <returns></returns>
        private static void LoadAllKeyValue(string section)
        {
            try
            {
                // 默认为32767（32.767KB），设置为128000000（128MB）
                uint MAX_BUFFER = 256000000;
                // 返回值[返回值形式为 key=value,例如 Color=Red]
                string[] items = new string[0];

                //分配内存
                IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

                uint bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, _AppIniFile);

                if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
                {
                    string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
                    items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                }

                // 释放内存
                Marshal.FreeCoTaskMem(pReturnedString);

                foreach (string entry in items)
                {
                    string[] v = entry.Split('=');

                    // 获取 section 节点
                    Dictionary<string, string> sectionDic = _AppIniCache.ContainsKey(section) ? _AppIniCache[section] : new Dictionary<string, string>();
                    // 设置 section 节点子项
                    if (sectionDic.ContainsKey(v[0]))
                        sectionDic[v[0]] = v[1];
                    else
                        sectionDic.Add(v[0], v[1]);
                    // 回写 section 节点
                    if (_AppIniCache.ContainsKey(section))
                        _AppIniCache[section] = sectionDic;
                    else
                        _AppIniCache.Add(section, sectionDic);
                }
                log.Debug("加载" + section + "节点下全部键值，总数：" + _AppIniCache.Count);
            }
            catch(Exception ex) {
                log.Fatal("配置文件加载失败：" + ex.Message, ex);
                AppCore.Restart();
            }
        }
        #endregion

        #region Win32API
        /// <summary>
        /// 将指定的键和值写到指定的节点，如果已经存在则替换
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpKeyName">键名称。如果为null，则删除指定的节点及其所有的项目</param>
        /// <param name="lpString">值内容。如果为null，则删除指定节点中指定的键。</param>
        /// <param name="lpFileName">INI文件</param>
        /// <returns>操作是否成功</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
        /// <summary>
        /// 读取INI文件中指定的Key的值
        /// </summary>
        /// <param name="lpAppName">节点名称。如果为null,则读取INI中所有节点名称,每个节点名称之间用\0分隔</param>
        /// <param name="lpKeyName">Key名称。如果为null,则读取INI中指定节点中的所有KEY,每个KEY之间用\0分隔</param>
        /// <param name="lpDefault">读取失败时的默认值</param>
        /// <param name="lpReturnedString">读取的内容缓冲区，读取之后，多余的地方使用\0填充</param>
        /// <param name="nSize">内容缓冲区的长度</param>
        /// <param name="lpFileName">INI文件名</param>
        /// <returns>实际读取到的长度</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [In, Out] char[] lpReturnedString, uint nSize, string lpFileName);

        //另一种声明方式,使用 StringBuilder 作为缓冲区类型的缺点是不能接受\0字符，会将\0及其后的字符截断,
        //所以对于lpAppName或lpKeyName为null的情况就不适用
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        //再一种声明，使用string作为缓冲区的类型同char[]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnedString, uint nSize, string lpFileName);
        /// <summary>
        /// 获取某个指定节点(Section)中所有KEY和Value
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpReturnedString">返回值的内存地址,每个之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// 获取所有节点名称(Section)
        /// </summary>
        /// <param name="lpszReturnBuffer">存放节点名称的内存地址,每个节点之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]

        private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

        /// <summary>
        /// 获取窗口线程进程ID
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        #endregion
    }
}
