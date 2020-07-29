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
    }
}
