using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.hideakin.textsearch.utility
{
    internal static class SearchResult
    {
        public static PathRowColumns[] ToArrayOfPathRowColumns(List<string> qTexts, List<PathRanges> rangesList)
        {
            var prcList = new List<PathRowColumns>();
            foreach (var ranges in rangesList)
            {
                List<RowColumns> rcList = ToLines(qTexts, ranges.Path, ranges.Ranges);
                if (rcList.Count > 0)
                {
                    prcList.Add(new PathRowColumns(ranges.Path, rcList));
                }
            }
            return prcList.ToArray();
        }

        private static List<RowColumns> ToLines(List<string> qTexts, string path, List<(int Start, int End)> ranges)
        {
            var dct = new Dictionary<int, List<(int Start, int End)>>();
            try
            {
                using (var sr = new StreamReader(path, true))
                {
                    var tokenizer = new Tokenizer();
                    tokenizer.Run(sr);
                    foreach (var range in ranges)
                    {
                        if (range.End < tokenizer.Tokens.Count)
                        {
                            if (tokenizer.Tokens[range.Start].Row == tokenizer.Tokens[range.End].Row)
                            {
                                List<(int Start, int End)> colRanges;
                                if (!dct.TryGetValue(tokenizer.Tokens[range.Start].Row, out colRanges))
                                {
                                    colRanges = new List<(int Start, int End)>();
                                    dct.Add(tokenizer.Tokens[range.Start].Row, colRanges);
                                }
                                if (qTexts.Count == 1)
                                {
                                    int index = tokenizer.Tokens[range.Start].Text.IndexOf(qTexts[0]);
                                    while (index >= 0)
                                    {
                                        int start = tokenizer.Tokens[range.Start].Column + index;
                                        int end = start + qTexts[0].Length;
                                        colRanges.Add((start, end));
                                        index = tokenizer.Tokens[range.Start].Text.IndexOf(qTexts[0], index + qTexts[0].Length);
                                    }
                                }
                                else if (qTexts.Count > 1)
                                {
                                    int start = tokenizer.Tokens[range.Start].Column + tokenizer.Tokens[range.Start].Text.Length - qTexts.First().Length;
                                    int end = tokenizer.Tokens[range.End].Column + qTexts.Last().Length;
                                    colRanges.Add((start, end));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            var list = new List<RowColumns>(dct.Count);
            foreach (var kv in dct)
            {
                list.Add(new RowColumns(kv.Key, kv.Value));
            }
            foreach (var e in list)
            {
                e.Columns.Sort((x, y) =>
                {
                    return x.Start < y.Start ? -1 :
                        x.Start > y.Start ? 1 :
                        x.End < y.End ? -1 :
                        x.End > y.End ? 1 :
                        0;
                });
            }
            list.Sort((x, y) =>
            {
                return x.Row < y.Row ? -1 :
                    x.Row > y.Row ? 1 :
                    0;
            });
            return list;
        }
    }
}
