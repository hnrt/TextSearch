using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.model
{
    public class HitRowColumns
    {
        public int Fid { get; }

        public List<RowColumns> Rows { get; }

        public HitRowColumns(int fid, List<RowColumns> rows)
        {
            Fid = fid;
            Rows = rows;
        }

        public static HitRowColumns[] And(HitRowColumns[] h1, HitRowColumns[] h2)
        {
            var h = new List<HitRowColumns>();
            foreach (var e1 in h1)
            {
                var e2 = h2.Where(x => x.Fid == e1.Fid).FirstOrDefault();
                if (e2 != null)
                {
                    var rows = new List<RowColumns>();
                    foreach (var f1 in e1.Rows)
                    {
                        var f2 = e2.Rows.Where(x => x.Row == f1.Row).FirstOrDefault();
                        if (f2 != null)
                        {
                            rows.Add(new RowColumns(f1.Row, f1.Columns, f2.Columns));
                        }
                    }
                    if (rows.Count > 0)
                    {
                        h.Add(new HitRowColumns(e1.Fid, rows));
                    }
                }
            }
            return h.ToArray();
        }

        public static HitRowColumns[] Or(HitRowColumns[] h1, HitRowColumns[] h2)
        {
            var h = new List<HitRowColumns>();
            foreach (var e1 in h1)
            {
                var e2 = h2.Where(x => x.Fid == e1.Fid).FirstOrDefault();
                if (e2 != null)
                {
                    var rows = new List<RowColumns>();
                    foreach (var f1 in e1.Rows)
                    {
                        var f2 = e2.Rows.Where(x => x.Row == f1.Row).FirstOrDefault();
                        if (f2 != null)
                        {
                            rows.Add(new RowColumns(f1.Row, f1.Columns, f2.Columns));
                        }
                    }
                    if (rows.Count > 0)
                    {
                        h.Add(new HitRowColumns(e1.Fid, rows));
                    }
                }
                else
                {
                    h.Add(e1);
                }
            }
            foreach (var e2 in h2)
            {
                var e1 = h1.Where(x => x.Fid == e2.Fid).FirstOrDefault();
                if (e1 == null)
                {
                    h.Add(e2);
                }
            }
            return h.ToArray();
        }
    }
}
