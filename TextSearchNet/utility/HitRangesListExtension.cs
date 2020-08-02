using com.hideakin.textsearch.model;
using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.utility
{
    internal static class HitRangesListExtension
    {
        public static List<HitRanges> ToList(IEnumerable<TextDistribution> collection)
        {
            var list = new List<HitRanges>(collection.Count());
            foreach (var entry in collection)
            {
                list.Add(new HitRanges(entry));
            }
            return list;
        }

        public static List<HitRanges> Merge(this List<HitRanges> list, IEnumerable<TextDistribution> collection)
        {
            foreach (var entry in collection)
            {
                var found = list.Where(x => x.Fid == entry.Fid).FirstOrDefault();
                if (found != null)
                {
                    found.Merge(entry);
                }
                else
                {
                    list.Add(new HitRanges(entry));
                }
            }
            return list;
        }

        public static List<HitRanges> Merge(this List<HitRanges> list, IEnumerable<HitRanges> collection)
        {
            foreach (var entry in collection)
            {
                var found = list.Where(x => x.Fid == entry.Fid).FirstOrDefault();
                if (found != null)
                {
                    found.Merge(entry);
                }
                else
                {
                    list.Add(entry);
                }
            }
            return list;
        }

        public static List<HitRanges> Append(this List<HitRanges> list, IEnumerable<TextDistribution> collection)
        {
            int index = 0;
            while(index < list.Count)
            {
                var current = list[index];
                var found = collection.Where(x => x.Fid == current.Fid).FirstOrDefault();
                if (found != null && current.Append(found).Ranges.Count > 0)
                {
                    index++;
                }
                else
                {
                    list.RemoveAt(index);
                }
            }
            return list;
        }
    }
}
