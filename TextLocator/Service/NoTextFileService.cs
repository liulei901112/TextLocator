namespace TextLocator.Service
{
    /// <summary>
    /// 无文本文件服务
    /// </summary>
    public class NoTextFileService : IFileInfoService
    {
        public string GetFileContent(string filePath)
        {
            return filePath;
        }
    }
}
