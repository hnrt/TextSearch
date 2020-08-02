using com.hideakin.textsearch.model;
using System.Collections.Generic;
using System.Linq;

namespace com.hideakin.textsearch.utility
{
    public static class HitFileListExtension
    {
        public static List<HitFile> Append(this List<HitFile> list, IEnumerable<HitFile> other)
        {
            int index = 0;
            while (index < list.Count)
            {
                var current = list[index];
                var found = other.Where(x => x.Fid == current.Fid).FirstOrDefault();
                if (found != null && current.Append(found).Rows.Count > 0)
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

        public static List<HitFile> And(this List<HitFile> list, IEnumerable<HitFile> other)
        {
            int index = 0;
            while (index < list.Count)
            {
                var current = list[index];
                var found = other.Where(x => x.Fid == current.Fid).FirstOrDefault();
                if (found != null && current.And(found).Rows.Count > 0)
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

        public static List<HitFile> Or(this List<HitFile> list, IEnumerable<HitFile> other)
        {
            foreach (var entry in other)
            {
                var found = list.Where(x => x.Fid == entry.Fid).FirstOrDefault();
                if (found != null)
                {
                    found.Or(entry);
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
