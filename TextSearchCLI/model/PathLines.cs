namespace com.hideakin.textsearch.model
{
    internal class PathLines
    {
        public string Path { get; set; }

        public int[] Lines { get; set; }

        public PathLines(string path, int[] lines)
        {
            Path = path;
            Lines = lines;
        }
    }
}
