namespace com.hideakin.textsearch.model
{
    public class SequenceExpression : QueryExpression
    {
        public string[] Texts { get; }

        public SequenceExpression(string[] texts)
            : base()
        {
            Texts = texts;
        }
    }
}
