using com.hideakin.textsearch.model;
using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.utility
{
    internal static class HitRangesListExtension
    {
        public static List<HitRanges> ToList(TextDistribution[] array)
        {
            var list = new List<HitRanges>(array.Length);
            foreach (var td in array)
            {
                list.Add(new HitRanges(td));
            }
            return list;
        }

        public static List<HitRanges> Merge(this List<HitRanges> list, TextDistribution[] array)
        {
            int index = 0;
            while(index < list.Count)
            {
                var found = array.Where(x => x.Fid == list[index].Fid).First();
                if (found != null)
                {
                    list[index].Merge(found);
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
