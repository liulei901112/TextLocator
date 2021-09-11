using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using System.Collections.Generic;
using System.IO;

namespace TextLocator.Jieba
{
    internal class JiebaAnalyzer : Analyzer
    {
        protected static readonly ISet<string> DefaultStopWords = StopAnalyzer.ENGLISH_STOP_WORDS_SET;

        private static ISet<string> StopWords;

        static JiebaAnalyzer()
        {
            var stopWordsFile = Path.GetFullPath(ConfigManager.StopWordsFile);
            if (File.Exists(stopWordsFile))
            {
                var lines = File.ReadAllLines(stopWordsFile);
                StopWords = new HashSet<string>();
                foreach (var line in lines)
                {
                    StopWords.Add(line.Trim());
                }
            }
            else
            {
                StopWords = DefaultStopWords;
            }
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var seg = new JiebaSegmenter();
            TokenStream result = new JiebaTokenizer(seg, reader);
            // 此筛选器是必需的，因为解析器将查询转换为小写形式
            result = new LowerCaseFilter(result);
            result = new StopFilter(true, result, StopWords);
            return result;
        }
    }
}
