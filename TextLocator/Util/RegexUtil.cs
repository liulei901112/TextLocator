using System.Text.RegularExpressions;

namespace TextLocator.Util
{
    /// <summary>
    /// 正则表达式工具类
    /// </summary>
    public class RegexUtil
    {
        /// <summary>
        /// 构造正则表达式
        /// </summary>
        /// <param name="regexText">正则表达式文本（也可以是纯文本普通内容）</param>
        /// <param name="matchCase">是否区分大小写，默认区分大小写</param>
        /// <returns></returns>
        public static Regex BuildRegex(string regexText, bool matchCase = true)
        {
            RegexOptions regexOptions = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            Regex regex = null;
            try
            {
                regex = new Regex(@"" + regexText, regexOptions);
            }
            catch
            {
                regex = new Regex(@"\" + regexText, regexOptions);
            }
            return regex;
        }
    }
}
