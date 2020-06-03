namespace com.hideakin.textsearch.model
{
    internal class Range
    {
        public int Start { get; set; }

        public int End { get; set; }

        public int Length => End - Start + 1;

        public Range()
        {
            Start = 0;
            End = 0;
        }

        public Range(int start)
        {
            Start = start;
            End = start;
        }
    }
}
