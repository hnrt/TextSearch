using com.hideakin.textsearch.data;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace com.hideakin.textsearch.model
{
    public abstract class QueryExpression
    {
        public QueryExpression()
        {
        }

        public abstract List<HitFile> Evaluate(
            Func<string, string[], SearchOptions, SearchOptions, List<HitFile>> search,
            string group,
            SearchOptions head = SearchOptions.EndsWith,
            SearchOptions tail = SearchOptions.StartsWith);

        public static QueryExpression Parse(string text)
        {
            using (var sr = new StringReader(text))
            {
                var tokenizer = new TextTokenizer(TextTokenizer.QUERY_MODE);
                tokenizer.Run(sr);
                var parser = new Parser();
                return parser.Run(tokenizer.Texts);
            }
        }

        private class Parser
        {
            private string[] texts;

            private int index;

            private string next;

            public Parser()
            {
            }

            public QueryExpression Run(string[] texts)
            {
                this.texts = texts;
                index = 0;
                Read();
                var expr = ParseLevel1();
                if (next != null)
                {
                    throw new FormatException("Unexpected end of expression.");
                }
                return expr;
            }

            private QueryExpression ParseLevel1()
            {
                var expr = ParseLevel2();
                if (next != "&")
                {
                    return expr;
                }
                var list = new List<QueryExpression> { expr };
                while (next == "&")
                {
                    Read();
                    list.Add(ParseLevel3());
                }
                return new AndExpression(list.ToArray());
            }

            private QueryExpression ParseLevel2()
            {
                var expr = ParseLevel3();
                if (next != "|")
                {
                    return expr;
                }
                var list = new List<QueryExpression> { expr };
                while (next == "|")
                {
                    Read();
                    list.Add(ParseLevel3());
                }
                return new OrExpression(list.ToArray());
            }

            private QueryExpression ParseLevel3()
            {
                var sequence = new List<QueryExpression>();
                if (next == null)
                {
                    throw new FormatException("Unexpected end of expression.");
                }
                else if (next == ")" || next == "&" || next == "|")
                {
                    throw new FormatException("Unexpected operator " + next + ".");
                }
                else if (next == "(")
                {
                    Read();
                    var expr = ParseLevel1();
                    if (next != ")")
                    {
                        throw new FormatException("Missing closing parenthesis.");
                    }
                    Read();
                    sequence.Add(expr);
                }
                else
                {
                    var texts = new List<string> { next };
                    Read();
                    while (next != null && next != "(" && next != ")" && next != "&" && next != "|")
                    {
                        texts.Add(next);
                        Read();
                    }
                    sequence.Add(new TextExpression(texts.ToArray()));
                }
                while (next != null && next != ")" && next != "&" && next != "|")
                {
                    if (next == "(")
                    {
                        Read();
                        var expr = ParseLevel1();
                        if (next != ")")
                        {
                            throw new FormatException("Missing closing parenthesis.");
                        }
                        Read();
                        sequence.Add(expr);
                    }
                    else
                    {
                        var texts = new List<string> { next };
                        Read();
                        while (next != null && next != "(" && next != ")" && next != "&" && next != "|")
                        {
                            texts.Add(next);
                            Read();
                        }
                        sequence.Add(new TextExpression(texts.ToArray()));
                    }
                }
                if (sequence.Count == 1)
                {
                    return sequence[0];
                }
                else
                {
                    return new SequenceExpression(sequence.ToArray());
                }
            }

            private void Read()
            {
                next = index < texts.Length ? texts[index++] : null;
            }
        }
    }
}
