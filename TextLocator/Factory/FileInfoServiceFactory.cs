using log4net;
using System;
using System.Collections.Generic;
using TextLocator.Enums;
using TextLocator.Service;

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

		/// <summary>
		/// 根据项目类型获取服务实例
		/// </summary>
		/// <param name="fileType">文件类型</param>
		/// <returns></returns>
		public static IFileInfoService GetFileInfoService(FileType fileType)
		{
			try
			{
				IFileInfoService fileInfoService = services[fileType];
				if (null == fileInfoService)
				{
					throw new Exception("暂无[" + fileType.ToString() + "]服务实例");
				}
				return fileInfoService;
			}
			catch (Exception ex)
			{
				log.Error(ex.Message, ex);
				throw new Exception("暂无[" + fileType.ToString() + "]服务实例");
			}
		}

		/// <summary>
		/// 注册服务实例
		/// </summary>
		/// <param name="fileType">文件类型</param>
		/// <param name="fileInfoService">服务实例</param>
		public static void Register(FileType fileType, IFileInfoService fileInfoService)
		{
			services.Add(fileType, fileInfoService);
		}
	}
}
