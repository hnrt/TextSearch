using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    internal class IndexService
    {
        private IndexNetClient netClient = new IndexNetClient();

        public IndexService()
        {
            netClient.Url = @"http://localhost:8080";
        }

        public bool UpdateIndex(string group, string path)
        {
            netClient.GroupName = group;
            using (var sr = new StreamReader(path, true))
            {
                var tokenizer = new Tokenizer();
                var texts = tokenizer.Run(sr);
                var req = new UpdateIndexRequest();
                req.Path = path;
                req.Texts = texts.ToArray();
                var task = netClient.UpdateIndex(req);
                task.Wait();
                var rsp = task.Result;
                return rsp != null;
            }
        }

        public PathLines[] FindText(string group, string text)
        {
            netClient.GroupName = group;
            using (var sr = new StringReader(text))
            {
                FindTextResponse rsp;
                var tokenizer = new Tokenizer();
                var texts = tokenizer.Run(sr);
                if (texts.Count == 0)
                {
                    return null;
                }
                else if (texts.Count == 1)
                {
                    var task = netClient.FindText(texts[0], SearchOptions.Contains);
                    task.Wait();
                    rsp = task.Result;
                }
                else
                {
                    var tasks = new Task<FindTextResponse>[texts.Count];
                    tasks[0] = netClient.FindText(texts[0], SearchOptions.EndsWith);
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        tasks[i] = netClient.FindText(texts[i], SearchOptions.Exact);
                    }
                    tasks[tasks.Length - 1] = netClient.FindText(texts[tasks.Length - 1], SearchOptions.StartsWith);
                    Task.WaitAll(tasks);
                    rsp = tasks[0].Result;
                    if (rsp != null)
                    {
                        for (int i = 1; i < tasks.Length; i++)
                        {
                            rsp.Merge(i, tasks[i].Result);
                        }
                    }
                }
                return ToPathLines(rsp.Hits);
            }
        }

        private PathLines[] ToPathLines(PathPositions[] pathPositions)
        {
            var pathLines = new PathLines[pathPositions.Length];
            for (int i = 0; i < pathLines.Length; i++)
            {
                pathLines[i] = new PathLines(pathPositions[i].Path, ToLines(pathPositions[i].Path, pathPositions[i].Positions));
            }
            return pathLines;
        }

        private int[] ToLines(string path, int[] positions)
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
