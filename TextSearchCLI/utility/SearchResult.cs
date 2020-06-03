using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.IO;

namespace com.hideakin.textsearch.utility
{
    internal static class SearchResult
    {
        public static PathLines[] ToPathLines(List<PathRanges> rangesList)
        {
            var linesList = new List<PathLines>();
            foreach (var ranges in rangesList)
            {
                List<int> lines = ToLines(ranges.Path, ranges.Ranges);
                if (lines.Count > 0)
                {
                    linesList.Add(new PathLines(ranges.Path, lines.ToArray()));
                }
            }
            return linesList.ToArray();
        }

        private static List<int> ToLines(string path, List<Range> ranges)
        {
            var lines = new List<int>();
            try
            {
                using (var sr = new StreamReader(path, true))
                {
                    var tokenizer = new Tokenizer();
                    tokenizer.Run(sr);
                    foreach (var range in ranges)
                    {
                        if (range.End < tokenizer.Texts.Count)
                        {
                            int lineStart = tokenizer.Lines[range.Start];
                            int lineEnd = tokenizer.Lines[range.End];
                            if (lineStart == lineEnd)
                            {
                                if (!lines.Contains(lineStart))
                                {
                                    lines.Add(lineStart);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return lines;
        }
    }
}
