using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextLocator.Jieba
{
    /// <summary>
    /// 分词器
    /// </summary>
    public class JiebaTokenizer : Tokenizer
    {
        private JiebaSegmenter segmenter;
        private ITermAttribute termAtt;
        private IOffsetAttribute offsetAtt;
        private ITypeAttribute typeAtt;

        private List<JiebaNet.Segmenter.Token> tokens;
        private int position = -1;

        public JiebaTokenizer(JiebaSegmenter seg, TextReader input) : this(seg, input.ReadToEnd()) { }

        public JiebaTokenizer(JiebaSegmenter seg, string input)
        {
            segmenter = seg;
            termAtt = AddAttribute<ITermAttribute>();
            offsetAtt = AddAttribute<IOffsetAttribute>();
            typeAtt = AddAttribute<ITypeAttribute>();

            var text = input;
            tokens = segmenter.Tokenize(text, TokenizerMode.Search).ToList();
        }

        public override bool IncrementToken()
        {
            ClearAttributes();
            position++;
            if (position < tokens.Count)
            {
                var token = tokens[position];
                termAtt.SetTermBuffer(token.Word);
                offsetAtt.SetOffset(token.StartIndex, token.EndIndex);
                typeAtt.Type = "Jieba";
                return true;
            }

            End();
            return false;
        }

        public IEnumerable<JiebaNet.Segmenter.Token> Tokenize(string text, TokenizerMode mode = TokenizerMode.Search)
        {
            return segmenter.Tokenize(text, mode);
        }
    }
}
