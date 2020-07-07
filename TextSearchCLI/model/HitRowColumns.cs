using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    internal class HitRowColumns
    {
        public int Fid { get; }

        public List<RowColumns> Rows { get; }

        public HitRowColumns(int fid, List<RowColumns> rows)
        {
            Fid = fid;
            Rows = rows;
        }
    }
}
