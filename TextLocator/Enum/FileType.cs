using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextLocator.Enum
{
    /// <summary>
    /// 文件类型
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Word
        /// </summary>
        [Description("doc,docm,docx")]
        Word类型,
        /// <summary>
        /// Excel
        /// </summary>
        [Description("xls,xlsm,xlsx")]
        Excel类型,
        /// <summary>
        /// PowerPoint
        /// </summary>
        [Description("ppt,pptm,pptx")]
        PowerPoint类型,
        /// <summary>
        /// PDF
        /// </summary>
        [Description("pdf")]
        PDF类型,
        /// <summary>
        /// XML和html
        /// </summary>
        [Description("htm,html,mht,mhtml")]
        HTML和XML类型,
        /// <summary>
        /// 纯文本
        /// </summary>
        [Description("txt,css,js,java,cs")]
        纯文本,
        /// <summary>
        /// 其他类型
        /// </summary>
        [Description()]
        其他类型
    }
}
