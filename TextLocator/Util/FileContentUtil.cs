using DocumentFormat.OpenXml.Office2016.Excel;
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
                        string regexText = keyword;
                        if (keyword.StartsWith(AppConst.REGEX_SEARCH_PREFIX))
                        {
                            regexText = keyword.Replace(AppConst.REGEX_SEARCH_PREFIX, "");
                        }
                        Regex regex = RegexUtil.BuildRegex(regexText, false);// new Regex(regexText, RegexOptions.IgnoreCase);
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

        /// <summary>
        /// 获取命中摘要列表
        /// </summary>
        /// <param name="content">内容文本</param>
        /// <param name="keywords">关键词列表</param>
        /// <param name="color">高亮色</param>
        /// <param name="isBackground">是否高亮背景</param>
        /// <param name="cutLength">切割长度</param>
        /// <returns></returns>
        public static FlowDocument GetHitBreviaryFlowDocument(string content, List<string> keywords, System.Windows.Media.Color color, bool isBackground = false, int cutLength = int.MinValue)
        {
            // 定义接收命中内容上下文的列表
            FlowDocument document = new FlowDocument();
            if (string.IsNullOrEmpty(content))
            {
                return document;
            }
            // 如果默认值，就使用参数定义值
            if (cutLength == int.MinValue)
            {
                cutLength = AppConst.FILE_CONTENT_BREVIARY_CUT_LENGTH;
            }

            // 内容（替换页码分隔符）
            content = AppConst.REGEX_CONTENT_PAGE.Replace(content, "");
            // 替换多余的换行
            content = AppConst.REGEX_LINE_BREAKS_WHITESPACE.Replace(content, " ");
            // 定义最大值和最小值、截取长度
            int min = 0;
            int max = content.Length;
            // 命中数索引下标
            int page = 1;
            // 遍历关键词列表
            foreach (string keyword in keywords)
            {
                string regexText = keyword;
                if (keyword.StartsWith(AppConst.REGEX_SEARCH_PREFIX))
                {
                    regexText = keyword.Replace(AppConst.REGEX_SEARCH_PREFIX, "");
                }
                // 定义关键词正则
                Regex regex = RegexUtil.BuildRegex(regexText, false);// new Regex(regexText, RegexOptions.IgnoreCase);
                // 匹配集合
                MatchCollection collection = regex.Matches(content);
                // 遍历命中列表
                foreach (Match match in collection)
                {
                    // 匹配位置
                    int index = match.Index;

                    int startIndex = index - cutLength / 2;
                    int endIndex = index + match.Length + cutLength / 2;

                    // 顺序不能乱
                    if (startIndex < min) startIndex = min;
                    if (endIndex > max) endIndex = max;
                    if (startIndex > endIndex) startIndex = endIndex - cutLength;
                    if (startIndex < min) startIndex = min;
                    if (startIndex + endIndex < cutLength) endIndex = endIndex + cutLength - (startIndex + endIndex);
                    if (endIndex > max) endIndex = max;

                    // 开始位置
                    string before = content.Substring(startIndex, index - startIndex);
                    if (startIndex > min)
                    {
                        before = "..." + before;
                    }
                    // 关键词位置（高亮处理）
                    string highlight = content.Substring(index, match.Length);
                    // 结束位置
                    string after = content.Substring(index + match.Length, endIndex - (index + match.Length));
                    if (endIndex < max)
                    {
                        after = after + "...";
                    }

                    Paragraph paragraph = new Paragraph();
                    // 摘要匹配位置序号                    
                    Run pageRun = new Run(string.Format("\n『{0}』\n", page));
                    pageRun.Background = new SolidColorBrush(Colors.DarkRed);
                    pageRun.Foreground = new SolidColorBrush(Colors.White);
                    paragraph.Inlines.Add(pageRun);
                    document.Blocks.Add(paragraph);

                    paragraph.FontSize = 13;
                    paragraph.FontFamily = new System.Windows.Media.FontFamily("微软雅黑");

                    Run beforeRun = new Run(before);
                    paragraph.Inlines.Add(beforeRun);

                    Run highlightRun = new Run(highlight);
                    highlightRun.FontWeight = FontWeight.FromOpenTypeWeight(700);
                    if (isBackground)
                    {
                        highlightRun.Background = new SolidColorBrush(color);
                        highlightRun.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        highlightRun.Foreground = new SolidColorBrush(color);
                    }

                    paragraph.Inlines.Add(highlightRun);

                    Run afterRun = new Run(after);
                    paragraph.Inlines.Add(afterRun);

                    /*// 分割线                    
                    Run pageRun = new Run(string.Format("\n------------------------------------------------------------------------------ {0}\n", page));
                    paragraph.Inlines.Add(pageRun);
                    document.Blocks.Add(paragraph);*/

                    page++;
                }
            }
            return document;
        }
    }
}
