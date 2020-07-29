namespace com.hideakin.textsearch.model
{
    public class AndExpression : QueryExpression
    {
        public QueryExpression[] Expressions { get; }

        public AndExpression(QueryExpression[] expressions)
            : base()
        {
            Expressions = expressions;
        }
    }
}
