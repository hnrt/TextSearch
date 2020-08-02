using com.hideakin.textsearch.data;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class SequenceExpression : QueryExpression
    {
        public QueryExpression[] Expressions { get; }

        public SequenceExpression(QueryExpression[] expressions)
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
            if (Expressions.Length == 1)
            {
                return Expressions[0].Evaluate(search, group, head, tail);
            }
            else
            {
                var c = Expressions[0].Evaluate(search, group, head, SearchOptions.Exact);
                for (int index = 1; index < Expressions.Length - 1; index++)
                {
                    c.Append(Expressions[index].Evaluate(search, group, SearchOptions.Exact, SearchOptions.Exact));
                }
                return c.Append(Expressions[Expressions.Length - 1].Evaluate(search, group, SearchOptions.Exact, tail));
            }
        }
    }
}
