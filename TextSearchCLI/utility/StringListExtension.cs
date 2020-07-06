using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.utility
{
    internal static class StringListExtension
    {
        public static List<string> MergeItems(this List<string> list, string items)
        {
            if (items != null)
            {
                var ss = items.Split(new char[] { ',' });
                foreach (string s in ss)
                {
                    if (!list.Contains(s))
                    {
                        list.Add(s);
                    }
                }
            }
            return list;
        }

        public static List<string> MergeItems(this List<string> list, string items, Func<string, string> validate)
        {
            if (items != null)
            {
                var ss = items.Split(new char[] { ',' });
                foreach (string s in ss)
                {
                    var t = validate(s);
                    if (!list.Contains(t))
                    {
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        public static string ToCsvString(this List<string> list)
        {
            var sb = new StringBuilder();
            foreach (string s in list)
            {
                sb.Append(",");
                sb.Append(s);
            }
            return sb.Length > 0 ? sb.ToString(1, sb.Length - 1) : "";
        }
    }
}
