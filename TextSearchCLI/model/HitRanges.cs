using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.model
{
    internal class HitRanges
    {
        public int Fid { get; set; }

        public List<(int Start, int End)> Ranges { get; }

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
            int index = 0;
            while (index < Ranges.Count)
            {
                var entry = Ranges[index];
                if (td.Positions.Contains(entry.End + 1))
                {
                    Ranges[index] = (entry.Start, entry.End + 1);
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
