using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.IO;

namespace com.hideakin.textsearch.utility
{
    internal static class SearchResult
    {
        public static PathLines[] ToPathLines(PathPositions[] pathPositions)
        {
            var pathLines = new PathLines[pathPositions.Length];
            for (int i = 0; i < pathLines.Length; i++)
            {
                pathLines[i] = new PathLines(pathPositions[i].Path, ToLines(pathPositions[i].Path, pathPositions[i].Positions));
            }
            return pathLines;
        }

        private static int[] ToLines(string path, int[] positions)
        {
            try
            {
                using (var sr = new StreamReader(path, true))
                {
                    var tokenizer = new Tokenizer();
                    var texts = tokenizer.Run(sr);
                    var lines = new List<int>();
                    int line = 0;
                    int found = 0;
                    for (int txtIdx = 0, posIdx = 0; txtIdx < texts.Count && posIdx < positions.Length; txtIdx++)
                    {
                        if (txtIdx == positions[posIdx])
                        {
                            posIdx++;
                            if (found == 0)
                            {
                                lines.Add(line);
                            }
                            found++;
                        }
                        if (texts[txtIdx] == Tokenizer.NEW_LINE)
                        {
                            line++;
                            found = 0;
                        }
                    }
                    return lines.ToArray();
                }
            }
            catch (Exception)
            {
                return new int[0];
            }
        }
    }
}
