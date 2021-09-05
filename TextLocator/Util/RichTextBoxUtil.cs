﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TextLocator.Util
{
    public class RichTextBoxUtil
    {
        /// <summary>
        /// 关键词高亮
        /// </summary>
        /// <param name="richTextBox">UI元素</param>
        /// <param name="color">颜色值</param>
        /// <param name="keywords">关键词</param>
        public static void Highlighted(RichTextBox richTextBox, Color color, List<string> keywords)
        {
            foreach (string keyword in keywords)
            {
                // 设置文字指针为Document初始位置           
                // richBox.Document.FlowDirection            
                TextPointer position = richTextBox.Document.ContentStart;
                while (position != null)
                {
                    //向前搜索,需要内容为Text                
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        //拿出Run的Text                    
                        string text = position.GetTextInRun(LogicalDirection.Forward);
                        //可能包含多个keyword,做遍历查找                    
                        int index = text.IndexOf(keyword, 0);
                        if (index != -1)
                        {
                            TextPointer start = position.GetPositionAtOffset(index);
                            TextPointer end = start.GetPositionAtOffset(keyword.Length);
                            position = selecta(richTextBox, color, keyword.Length, start, end);
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
        /// <param name="selectLen">选择长度</param>
        /// <param name="tpStart">内容指针开始位置</param>
        /// <param name="tpEnd">内容指针结束位置</param>
        /// <returns></returns>
        private static TextPointer selecta(RichTextBox richTextBox, Color color, int selectLen, TextPointer tpStart, TextPointer tpEnd)
        {
            TextRange range = richTextBox.Selection;
            range.Select(tpStart, tpEnd);

            //高亮选择
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

            return tpEnd.GetNextContextPosition(LogicalDirection.Forward);
        }
    }
}