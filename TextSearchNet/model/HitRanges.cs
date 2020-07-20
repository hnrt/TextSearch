using System;
using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.model
{
    internal class HitRanges
    {
        public int Fid { get; set; }

        public List<(int Start, int End)> Ranges { get; private set; }

        public HitRanges()
        {
            Fid = -1;
            Ranges = new List<(int Start, int End)>();
        }

        public HitRanges(int fid)
        {
            Fid = fid;
            Ranges = new List<(int Start, int End)>();
        }

        public HitRanges(TextDistribution td)
        {
            Fid = td.Fid;
            Ranges = new List<(int Start, int End)>(td.Positions.Length);
            foreach (int position in td.Positions)
            {
                Ranges.Add((position, position));
            }
        }

        public HitRanges Merge(TextDistribution td)
        {
            if (td.Positions.Length == 0)
            {
                return this;
            }
            if (Ranges.Count == 0)
            {
                foreach (int position in td.Positions)
                {
                    Ranges.Add((position, position));
                }
                return this;
            }
            var r = new List<(int Start, int End)>(Ranges.Count + td.Positions.Length);
            int a = Ranges[0].Start;
            int b = td.Positions[0];
            int i = 1;
            int j = 1;
            while (true)
            {
                if (a < b)
                {
                    r.Add((a, a));
                    if (i < Ranges.Count)
                    {
                        a = Ranges[i++].Start;
                    }
                    else
                    {
                        r.Add((b, b));
                        while (j < td.Positions.Length)
                        {
                            b = td.Positions[j++];
                            r.Add((b, b));
                        }
                        break;
                    }
                }
                else if (a > b)
                {
                    r.Add((b, b));
                    if (j < td.Positions.Length)
                    {
                        b = td.Positions[j++];
                    }
                    else
                    {
                        r.Add((a, a));
                        while (i < Ranges.Count)
                        {
                            a = Ranges[i++].Start;
                            r.Add((a, a));
                        }
                        break;
                    }
                }
                else
                {
                    throw new Exception("HitRanges.Merge: Duplicate position.");
                }
            }
            Ranges = r;
            return this;
        }

        public HitRanges AddNext(TextDistribution td)
        {
            int index = 0;
            while (index < Ranges.Count)
            {
                var (start, end) = Ranges[index];
                if (td.Positions.Contains(end + 1))
                {
                    Ranges[index] = (start, end + 1);
                    index++;
                }
                else
                {
                    Ranges.RemoveAt(index);
                }
            }
            return this;
        }
    }
}
