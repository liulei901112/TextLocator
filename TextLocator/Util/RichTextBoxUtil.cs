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
    public class RichTextBoxUtil
    {

        /// <summary>
        /// 填充数据
        /// </summary>
        /// <param name="richTextBox">RichTextBox文本对象</param>
        /// <param name="text">高亮关键词</param>
        /// <param name="brush">高亮颜色画刷</param>
        /// <param name="underLine">下划线</param>
        public static void FillingData(RichTextBox richTextBox, string text, Brush brush, bool underLine = false)
        {
            richTextBox.Document.Blocks.Clear();
            // Paragraph 类似于 html 的 P 标签
            Paragraph paragraph = new Paragraph();
            // Run 是一个 Inline 的标签
            Run run = new Run(text);
            run.Foreground = brush;
            if (underLine)
            {
                run.TextDecorations = TextDecorations.Underline;
            }
            paragraph.Inlines.Add(run);
            richTextBox.Document.Blocks.Add(paragraph);
        }

        /// <summary>
        /// 关键词高亮
        /// </summary>
        /// <param name="richTextBox">UI元素</param>
        /// <param name="color">颜色值</param>
        /// <param name="keywords">关键词</param>
        /// <param name="background">背景颜色</param>
        public static void Highlighted(RichTextBox richTextBox, Color color, List<string> keywords, bool background = false)
        {
            if (keywords == null || keywords.Count <= 0) return;
            foreach (string keyword in keywords)
            {
                if (string.IsNullOrEmpty(keyword)) continue;
                // 设置文字指针为Document初始位置           
                // richBox.Document.FlowDirection            
                TextPointer position = richTextBox.Document.ContentStart;
                while (position != null)
                {
                    // 向前搜索,需要内容为Text                
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        // 拿出Run的Text                    
                        string text = position.GetTextInRun(LogicalDirection.Forward);
                        // 关键词是正则表达式
                        if (AppConst.REGEX_SUPPORT_WILDCARDS.IsMatch(keyword))
                        {
                            Regex regex = new Regex(keyword, RegexOptions.IgnoreCase);
                            Match matches = regex.Match(text);
                            if (matches.Success)
                            {
                                TextPointer start = position.GetPositionAtOffset(matches.Index);
                                TextPointer end = start.GetPositionAtOffset(matches.Length);
                                position = Selecta(richTextBox, color, start, end, background);
                            }
                        }
                        else
                        {
                            // 可能包含多个keyword,做遍历查找                    
                            int index = text.IndexOf(keyword, 0, StringComparison.CurrentCultureIgnoreCase);
                            if (index != -1)
                            {
                                TextPointer start = position.GetPositionAtOffset(index);
                                TextPointer end = start.GetPositionAtOffset(keyword.Length);
                                position = Selecta(richTextBox, color, start, end, background);
                            }
                        }
                    }
                    // 文字指针向前偏移
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
        }

        /// <summary>
        /// 改变关键字的具体实现
        /// </summary>
        /// <param name="richTextBox">元素</param>
        /// <param name="color">颜色</param>
        /// <param name="tpStart">内容指针开始位置</param>
        /// <param name="tpEnd">内容指针结束位置</param>
        /// <returns></returns>
        private static TextPointer Selecta(RichTextBox richTextBox, Color color, TextPointer tpStart, TextPointer tpEnd, bool background)
        {
            TextRange range = richTextBox.Selection;
            range.Select(tpStart, tpEnd);

            //高亮选择
            if (background)
            {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));
                range.ApplyPropertyValue(TextElement.FontWeightProperty, color == Colors.White ? FontWeights.Normal : FontWeights.Bold);
            }
            else
            {
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
                range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
            

            return tpEnd.GetNextContextPosition(LogicalDirection.Forward);
        }
    }
}
