using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    internal class PathRanges
    {
        public string Path { get; set; }

        public List<Range> Ranges { get; } = new List<Range>();

        public PathRanges()
        {
        }

        public PathRanges(PathPositions pp)
        {
            Path = pp.Path;
            foreach (int position in pp.Positions)
            {
                Ranges.Add(new Range(position));
            }
        }

        public static List<PathRanges> ToList(PathPositions[] pparray)
        {
            var list = new List<PathRanges>();
            foreach (var pp in pparray)
            {
                list.Add(new PathRanges(pp));
            }
            return list;
        }

        public static List<PathRanges> Merge(List<PathRanges> srcList, PathPositions[] pparray)
        {
            var dstList = new List<PathRanges>();
            foreach (var entry in srcList)
            {
                for (int index = 0; index < pparray.Length; index++)
                {
                    if (pparray[index].Path == entry.Path)
                    {
                        var result = Merge(entry, pparray[index]);
                        if (result != null)
                        {
                            dstList.Add(result);
                        }
                    }
                }
            }
            return dstList;
        }

        private static PathRanges Merge(PathRanges entry, PathPositions pp)
        {
            var result = new PathRanges();
            result.Path = entry.Path;
            foreach (var range in entry.Ranges)
            {
                for (int index = 0; index < pp.Positions.Length; index++)
                {
                    if (pp.Positions[index] == range.End + 1)
                    {
                        range.End = pp.Positions[index];
                        result.Ranges.Add(range);
                        break;
                    }
                }
            }
            return result.Ranges.Count > 0 ? result : null;
        }
    }
}
