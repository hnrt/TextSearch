namespace com.hideakin.textsearch.model
{
    /// <summary>
    /// A token is defined as the longest possible character sequence that do not contain any embedded delimiters.
    /// </summary>
    internal class Token
    {
        public string Text { get; }

        public int Row { get; }

        public int Column { get; }

        public Token(string text, int row, int column)
        {
            Text = text;
            Row = row;
            Column = column;
        }
    }
}
