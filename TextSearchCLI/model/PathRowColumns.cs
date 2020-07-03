using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    internal class PathRowColumns
    {
        public int Fid { get; }

        public string Path { get; }

        public List<RowColumns> Rows { get; }

        public PathRowColumns(int fid, string path, List<RowColumns> rows)
        {
            Fid = fid;
            Path = path;
            Rows = rows;
        }
    }
}
