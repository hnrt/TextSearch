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

        public static List<HitRanges> AddNext(this List<HitRanges> list, IEnumerable<TextDistribution> collection)
        {
            int index = 0;
            while(index < list.Count)
            {
                var found = collection.Where(x => x.Fid == list[index].Fid).FirstOrDefault();
                if (found != null)
                {
                    list[index++].AddNext(found);
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
