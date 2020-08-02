using com.hideakin.textsearch.data;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class OrExpression : QueryExpression
    {
        public QueryExpression[] Expressions { get; }

        public OrExpression(QueryExpression[] expressions)
            : base()
        {
            Expressions = expressions;
        }

        public override List<HitFile> Evaluate(
            Func<string, string[], SearchOptions, SearchOptions, List<HitFile>> search,
            string group,
            SearchOptions head = SearchOptions.EndsWith,
            SearchOptions tail = SearchOptions.StartsWith)
        {
            var c = Expressions[0].Evaluate(search, group, head, tail);
            for (int index = 1; index < Expressions.Length; index++)
            {
                c.Or(Expressions[index].Evaluate(search, group, head, tail));
            }
            return c;
        }
    }
}
