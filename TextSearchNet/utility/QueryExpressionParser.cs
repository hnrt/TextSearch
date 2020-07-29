using com.hideakin.textsearch.model;
using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.utility
{
    internal class QueryExpressionParser
    {
        private string[] texts;

        private int index;

        private string next;

        public QueryExpressionParser()
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
            var list = new List<QueryExpression>();
            list.Add(expr);
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
            var list = new List<QueryExpression>();
            list.Add(expr);
            while (next == "|")
            {
                Read();
                list.Add(ParseLevel3());
            }
            return new OrExpression(list.ToArray());
        }

        private QueryExpression ParseLevel3()
        {
            if (next == null)
            {
                throw new FormatException("Unexpected end of expression.");
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
                return expr;
            }
            else if (next == ")" || next == "&" || next == "|")
            {
                throw new FormatException("Unexpected operator " + next + ".");
            }
            else
            {
                var sequence = new List<string>();
                sequence.Add(next);
                Read();
                while (next != null && next != "(" && next != ")" && next != "&" && next != "|")
                {
                    sequence.Add(next);
                    Read();
                }
                return new SequenceExpression(sequence.ToArray());
            }
        }

        private void Read()
        {
            next = index < texts.Length ? texts[index++] : null;
        }
    }
}
