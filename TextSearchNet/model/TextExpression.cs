using com.hideakin.textsearch.data;
using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class TextExpression : QueryExpression
    {
        public string[] Texts { get; }

        public TextExpression(string[] texts)
            : base()
        {
            Texts = texts;
        }

        public override List<HitFile> Evaluate(
            Func<string, string[], SearchOptions, SearchOptions, List<HitFile>> search,
            string group,
            SearchOptions head = SearchOptions.EndsWith,
            SearchOptions tail = SearchOptions.StartsWith)
        {
            return search(group, Texts, head, tail);
        }
    }
}
