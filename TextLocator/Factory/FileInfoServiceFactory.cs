using log4net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TextLocator.Core;
using TextLocator.Enums;
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
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string GetFileContent(string filePath)
		{
			// 获取文件服务对象
			IFileInfoService fileInfoService = GetFileInfoService(FileTypeUtil.GetFileType(filePath));

			string content;
			// 读取文件内容
			content = WaitTimeout(fileInfoService.GetFileContent, filePath, TimeSpan.FromSeconds(AppConst.FILE_READ_TIMEOUT));
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
				throw new Exception("暂无[" + fileType.ToString() + "]服务实例， 返回默认其他类型文件服务实例");
			}
		}
		#endregion

		#region 超时函数
		/// <summary>
		/// 有参数,有反回值方法
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public delegate string TimeoutDelegate(string param);

		/// <summary>
		/// 有参数,有反回值超时方法
		/// </summary>
		/// <param name="Method">执行方法</param>
		/// <param name="OutTime">超时时间</param>
		/// <param name="Params">执行参数</param>
		/// <returns>反回一个string类型方法</returns>
		public static string WaitTimeout(TimeoutDelegate method, string param, TimeSpan timeout)
		{
			string obj = null;
			AutoResetEvent are = new AutoResetEvent(false);
			Thread t = new Thread(delegate () { obj = method(param); are.Set(); });
			t.Start();
			Wait(t, timeout, are);
			return obj;
		}

		/// <summary>
		/// 等待方法执行完成,或超时
		/// </summary>
		/// <param name="t"></param>
		/// <param name="OutTime"></param>
		/// <param name="ares"></param>
		private static void Wait(Thread t, TimeSpan timeout, WaitHandle are)
		{
			WaitHandle[] ares = new WaitHandle[] { are };
			int index = WaitHandle.WaitAny(ares, timeout);
			if ((index != 0) && t.IsAlive) // 如果不是执行完成的信号,并且,线程还在执行,那么,结束这个线程
			{
				t.Abort();
			}
		}
		#endregion
	}
}
