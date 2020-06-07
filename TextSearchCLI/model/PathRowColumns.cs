using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    internal class PathRowColumns
    {
        public string Path { get; }

        public List<RowColumns> Rows { get; }

        public PathRowColumns(string path, List<RowColumns> rows)
        {
            Path = path;
            Rows = rows;
        }
    }
}
