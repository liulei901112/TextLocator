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
        Word类型,
        /// <summary>
        /// Excel
        /// </summary>
        [Description("xls,xlsx")]
        Excel类型,
        /// <summary>
        /// PowerPoint
        /// </summary>
        [Description("ppt,pptx")]
        PowerPoint类型,
        /// <summary>
        /// PDF
        /// </summary>
        [Description("pdf")]
        PDF类型,
        /// <summary>
        /// XML和html
        /// </summary>
        [Description("html,xml")]
        HTML和XML类型,
        /// <summary>
        /// 常用图片
        /// </summary>
        [Description("jpg,png,gif,jpeg")]
        常用图片,
        /// <summary>
        /// 代码文件
        /// </summary>
        [Description("css,js,java,cs,md")]
        代码文件,
        /// <summary>
        /// 纯文本
        /// </summary>
        [Description("txt")]
        纯文本,
        /// <summary>
        /// 其他类型
        /// </summary>
        [Description()]
        其他类型
    }
}
