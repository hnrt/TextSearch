using com.hideakin.textsearch.model;
using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.utility
{
    public static class TextDistributionListExtension
    {
        public static List<TextDistribution> Merge(this List<TextDistribution> list, IEnumerable<TextDistribution> collection)
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
    }
}
