using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Xps.Packaging;
using TextLocator.Core;
using TextLocator.Enums;
using TextLocator.Exceptions;
using TextLocator.Service;
using TextLocator.Util;

namespace TextLocator.Factory
{
    /// <summary>
    /// 文件信息服务工厂
    /// </summary>
    public class FileInfoServiceFactory
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// 服务字典
		/// </summary>
		private static Dictionary<FileType, IFileInfoService> services = new Dictionary<FileType, IFileInfoService>();

		#region 工厂方法
		/// <summary>
		/// 获取文件内容
		/// </summary>
		/// <param name="filePath">文件地址</param>
		/// <returns></returns>
		public static string GetFileContent(string filePath)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			try
			{
				// 如果文件存在
				if (fileInfo == null && !fileInfo.Exists)
				{
					throw new FileNotFoundException("文件未找到，请确认");
				}
				// 文件太大
				if (fileInfo.Length > AppConst.FILE_SIZE_LIMIT)
				{
					throw new FileBigSizeException("不支持大于 " + FileUtil.GetFileSizeFriendly(AppConst.FILE_SIZE_LIMIT) + " 的文件解析");
				}
			}
			catch (Exception ex)
			{
				log.Error(filePath + "->" + ex.Message, ex);

				return null;
			}

			// 获取文件服务对象
			IFileInfoService fileInfoService = GetFileInfoService(FileTypeUtil.GetFileType(filePath));

			// 内容
			string content = "";
			// 缓存Key
			string cacheKey = MD5Util.GetMD5Hash(filePath);

			if (CacheUtil.Exsits(cacheKey))
			{
				// 从缓存中读取
				content = CacheUtil.Get<string>(cacheKey);
#if DEBUG
				log.Debug(filePath + "，缓存生效。");
#endif
			}
			else
			{
				// 读取文件内容
				content = WaitTimeout(fileInfoService.GetFileContent, filePath);

				// 写入缓存
				CacheUtil.Put(cacheKey, content);
			}

			return content;
		}

		/// <summary>
		/// 注册服务实例
		/// </summary>
		/// <param name="fileType">文件类型</param>
		/// <param name="fileInfoService">服务实例</param>
		public static void Register(FileType fileType, IFileInfoService fileInfoService)
		{
			log.Debug((int)fileType + "：" + fileType.ToString() + " 引擎注册");
			services.Add(fileType, fileInfoService);
		}

		/// <summary>
		/// 根据项目类型获取服务实例
		/// </summary>
		/// <param name="fileType">文件类型</param>
		/// <returns></returns>
		private static IFileInfoService GetFileInfoService(FileType fileType)
		{
			try
			{
				return services[fileType];
			}
			catch (Exception ex)
			{
				log.Error(ex.Message, ex);
				throw new NotFoundFileServiceException("暂无[" + fileType.ToString() + "]服务实例， 返回默认其他类型文件服务实例");
			}
		}
		#endregion

		#region 超时函数
		/// <summary>
		/// 有参数,有反回值方法
		/// </summary>
		/// <param name="filePath">文件路径</param>
		/// <returns></returns>
		public delegate string TimeoutDelegate(string filePath);

		/// <summary>
		/// 有参数,有反回值超时方法
		/// </summary>
		/// <param name="method">执行方法</param>
		/// <param name="filePath">文件路径</param>
		/// <param name="timeout">超时时间</param>
		/// <returns>反回一个string类型方法</returns>
		public static string WaitTimeout(TimeoutDelegate method, string filePath)
		{
			string obj = null;
			AutoResetEvent are = new AutoResetEvent(false);
			Thread t = new Thread(delegate () { obj = method(filePath); are.Set(); });
			t.Start();
			Wait(t, are);
			return obj;
		}

		/// <summary>
		/// 等待方法执行完成,或超时
		/// </summary>
		/// <param name="t"></param>
		/// <param name="OutTime"></param>
		/// <param name="ares"></param>
		private static void Wait(Thread t, WaitHandle are)
		{
			WaitHandle[] ares = new WaitHandle[] { are };
			int index = WaitHandle.WaitAny(ares, TimeSpan.FromSeconds(AppConst.FILE_READ_TIMEOUT));
			if ((index != 0) && t.IsAlive) // 如果不是执行完成的信号,并且,线程还在执行,那么,结束这个线程
			{
				t.Abort();
			}
		}
		#endregion
	}
}
