using Contrib.Regex;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using System;
using System.Text;

namespace TextLocator.Index
{
    /// <summary>
    /// 基于正则表达式的查询。
    /// </summary>
    internal class RegexQuery : MultiTermQuery, IRegexQueryCapable, IEquatable<RegexQuery>
    {
        private IRegexCapabilities _regexImpl = new CSharpRegexCapabilities();
        public IRegexCapabilities RegexImplementation { get => _regexImpl; set => _regexImpl = value; }
        public Term Term { get; private set; }

        public RegexQuery(Term term)
        {
            Term = term;
        }

        public bool Equals(RegexQuery other)
        {
            if (other == null) return false;
            if (this == other) return true;

            if (!base.Equals(other)) return false;
            return _regexImpl.Equals(other._regexImpl);
        }

        public override string ToString(string field)
        {
            StringBuilder buffer = new StringBuilder();
            if (!Term.Field.Equals(field))
            {
                buffer.Append(Term.Field);
                buffer.Append(":");
            }
            buffer.Append(Term.Text);
            buffer.Append(ToStringUtils.Boost(Boost));
            return buffer.ToString();
        }

        protected override FilteredTermEnum GetEnum(IndexReader reader)
        {
            return new RegexTermEnum(reader, Term, _regexImpl);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || (obj as RegexQuery == null)) return false;
            if (this == obj) return true;

            return Equals((RegexQuery)obj);
        }

        public override int GetHashCode()
        {
            return 29 * base.GetHashCode() + _regexImpl.GetHashCode();
        }
    }
}
