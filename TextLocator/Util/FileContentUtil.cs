using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TextLocator.Core;

namespace TextLocator.Util
{
    public class FileContentUtil
    {
        #region RichText操作
        /// <summary>
        /// 清空RichText的Document
        /// </summary>
        /// <param name="richTextBox">RichTextBox对象</param>
        public static void EmptyRichTextDocument(RichTextBox richTextBox)
        {
            richTextBox.Document.Blocks.Clear();
        }

        /// <summary>
        /// 填充FlowDocument内容
        /// </summary>
        /// <param name="control">RichTextBox 或 FlowDocumentReader对象</param>
        /// <param name="text">高亮关键词</param>
        /// <param name="brush">高亮颜色画刷</param>
        /// <param name="underLine">下划线</param>
        public static void FillFlowDocument(Control control, string text, Brush brush, bool isUnderLine = false)
        {
            // Paragraph 类似于 html 的 P 标签
            Paragraph paragraph = new Paragraph();
            paragraph.FontFamily = new FontFamily("微软雅黑");
            paragraph.FontSize = 13;
            paragraph.Foreground = brush;
            if (isUnderLine)
            {
                paragraph.TextDecorations = TextDecorations.Underline;
            }
            // Run 是一个 Inline 的标签
            Run run = new Run(text);
            paragraph.Inlines.Add(run);
            if (control is RichTextBox)
            {
                (control as RichTextBox).Document = new FlowDocument(paragraph);
            }
            else
            {
                (control as FlowDocumentReader).Document = new FlowDocument(paragraph);
            }
        }

        /// <summary>
        /// FlowDocument关键词高亮
        /// </summary>
        /// <param name="control">RichTextBox 或 FlowDocumentReader对象</param>
        /// <param name="color">颜色值</param>
        /// <param name="keywords">关键词</param>
        /// <param name="isBackground">背景颜色</param>
        public static void FlowDocumentHighlight(Control control, Color color, List<string> keywords, bool isBackground = false)
        {
            if (keywords == null || keywords.Count <= 0) return;
            foreach (string keyword in keywords)
            {
                if (string.IsNullOrEmpty(keyword)) continue;
                // 设置文字指针为Document初始位置         
                TextPointer position;
                if (control is RichTextBox)
                {
                    position = (control as RichTextBox).Document.ContentStart;
                }
                else
                {
                    position = (control as FlowDocumentReader).Document.ContentStart;
                }
                while (position != null)
                {
                    // 向前搜索,需要内容为Text                
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        // 拿出Run的Text                    
                        string text = position.GetTextInRun(LogicalDirection.Forward);
                        // 关键词匹配查找
                        Regex regex = new Regex(keyword, RegexOptions.IgnoreCase);
                        Match matches = regex.Match(text);
                        if (matches.Success)
                        {
                            TextPointer start = position.GetPositionAtOffset(matches.Index);
                            TextPointer end = start.GetPositionAtOffset(matches.Length);
                            position = Selecta(control, color, start, end, isBackground);
                        }
                    }
                    // 文字指针向前偏移
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
        }

        /// <summary>
        /// FlowDocument关键词高亮的具体实现
        /// </summary>
        /// <param name="control">RichTextBox 或 FlowDocumentReader对象</param>
        /// <param name="color">颜色</param>
        /// <param name="tpStart">内容指针开始位置</param>
        /// <param name="tpEnd">内容指针结束位置</param>
        /// <returns></returns>
        private static TextPointer Selecta(Control control, Color color, TextPointer tpStart, TextPointer tpEnd, bool background)
        {
            TextRange range;
            if (control is RichTextBox)
            {
                range = (control as RichTextBox).Selection;
            }
            else
            {
                range = (control as FlowDocumentReader).Selection;
            }
            range.Select(tpStart, tpEnd);

            // 内容加粗
            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            // 内容高亮
            range.ApplyPropertyValue(background ? TextElement.BackgroundProperty : TextElement.ForegroundProperty, new SolidColorBrush(color));

            return tpEnd.GetNextContextPosition(LogicalDirection.Forward);
        }
        #endregion
    }
}
