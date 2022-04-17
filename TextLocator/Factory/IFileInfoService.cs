using System.Windows.Xps.Packaging;
using TextLocator.Factory;

namespace TextLocator.Service
{
    /// <summary>
    /// 文件服务
    /// </summary>
    public interface IFileInfoService
    {
        /// <summary>
        /// 获取文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        string GetFileContent(string filePath);
    }
}
