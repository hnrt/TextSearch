using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class HitFile
    {
        public int Fid { get; }

        public List<RowColumns> Rows { get; private set; }

        public HitFile(int fid, List<RowColumns> rows)
        {
            Fid = fid;
            Rows = rows;
        }

        public HitFile Append(HitFile other)
        {
            if (other.Fid != Fid)
            {
                throw new ArgumentException("FIDs mismatch.");
            }
            Rows = RowColumns.Append(Rows, other.Rows);
            return this;
        }

        public HitFile And(HitFile other)
        {
            if (other.Fid != Fid)
            {
                throw new ArgumentException("FIDs mismatch.");
            }
            Rows = RowColumns.And(Rows, other.Rows);
            return this;
        }

        public HitFile Or(HitFile other)
        {
            if (other.Fid != Fid)
            {
                throw new ArgumentException("FIDs mismatch.");
            }
            Rows = RowColumns.Or(Rows, other.Rows);
            return this;
        }
    }
}
