using System.ComponentModel;

namespace TextLocator.Enums
{
    /// <summary>
    /// 文件类型
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Word
        /// </summary>
        [Description("doc,docx")]
        Word文档,
        /// <summary>
        /// Excel
        /// </summary>
        [Description("xls,xlsx")]
        Excel表格,
        /// <summary>
        /// PowerPoint
        /// </summary>
        [Description("ppt,pptx")]
        PPT文稿,
        /// <summary>
        /// PDF
        /// </summary>
        [Description("pdf")]
        PDF文档,
        /// <summary>
        /// XML和html
        /// </summary>
        [Description("html,xml")]
        HTML和XML,
        /// <summary>
        /// 常用图片
        /// </summary>
        [Description("jpg,png,gif,jpeg")]
        常用图片,
        /// <summary>
        /// 代码文件
        /// </summary>
        [Description("cs,java,js,css,md")]
        代码文件,
        /// <summary>
        /// 纯文本
        /// </summary>
        [Description("txt")]
        文本文件
    }
}
