using System.Collections.Generic;

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

        public static List<HitRanges> ToList(TextDistribution[] array)
        {
            var list = new List<HitRanges>(array.Length);
            foreach (var td in array)
            {
                list.Add(new HitRanges(td));
            }
            return list;
        }

        public static List<HitRanges> Merge(List<HitRanges> srcList, TextDistribution[] array)
        {
            var dstList = new List<HitRanges>();
            foreach (var entry in srcList)
            {
                for (int index = 0; index < array.Length; index++)
                {
                    if (array[index].Fid == entry.Fid)
                    {
                        var result = Merge(entry, array[index]);
                        if (result != null)
                        {
                            dstList.Add(result);
                        }
                    }
                }
            }
            return dstList;
        }

        private static HitRanges Merge(HitRanges entry, TextDistribution td)
        {
            var result = new HitRanges(entry.Fid);
            foreach (var range in entry.Ranges)
            {
                for (int index = 0; index < td.Positions.Length; index++)
                {
                    if (td.Positions[index] == range.End + 1)
                    {
                        result.Ranges.Add((range.Start, td.Positions[index]));
                        break;
                    }
                }
            }
            return result.Ranges.Count > 0 ? result : null;
        }
    }
}
