using System.ComponentModel;

namespace TextLocator.Enums
{
    /// <summary>
    /// 文件类型
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Word文档
        /// </summary>
        [Description("doc,docx")]
        Word文档,
        /// <summary>
        /// Excel表格
        /// </summary>
        [Description("xls,xlsx")]
        Excel表格,
        /// <summary>
        /// PowerPoint
        /// </summary>
        [Description("ppt,pptx")]
        PPT文稿,
        /// <summary>
        /// PDF文档
        /// </summary>
        [Description("pdf")]
        PDF文档,
        /// <summary>
        /// XML和html
        /// </summary>
        [Description("html,xml")]
        DOM文档,
        /// <summary>
        /// 图片
        /// </summary>
        [Description("jpg,png,gif,jpeg")]
        图片,
        /// <summary>
        /// 代码
        /// </summary>
        [Description("cs,java,js,css,md,py")]
        代码,
        /// <summary>
        /// 纯文本
        /// </summary>
        [Description("txt")]
        纯文本
    }
}
