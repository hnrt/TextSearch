using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    internal class IndexService
    {
        private IndexNetClient NetClient { get; } = IndexNetClient.Instance;

        public IndexService()
        {
        }

        public PathLines[] FindText(string group, string text)
        {
            NetClient.GroupName = group;
            using (var sr = new StringReader(text))
            {
                List<PathRanges> rangesList = null;
                var tokenizer = new Tokenizer();
                tokenizer.Run(sr);
                var texts = tokenizer.Texts;
                if (Program.DebugLevel > 0)
                {
                    Console.WriteLine("#query: {0}", text);
                    int index = 0;
                    foreach (var t in texts)
                    {
                        Console.WriteLine("#{0,6} {1}", index++, t);
                    }
                }
                if (texts.Count == 1)
                {
                    var task = NetClient.FindText(texts[0], SearchOptions.Contains);
                    task.Wait();
                    if (task.Result != null)
                    {
                        if (Program.DebugLevel > 0)
                        {
                            foreach (var pp in task.Result.Hits)
                            {
                                Console.WriteLine("#{0}", pp.Path);
                                foreach (var position in pp.Positions)
                                {
                                    Console.WriteLine("#{0,6} {1}", position, texts[0]);
                                }
                            }
                        }
                        rangesList = PathRanges.ToList(task.Result.Hits);
                    }
                }
                else if(texts.Count > 1)
                {
                    var tasks = new Task<FindTextResponse>[texts.Count];
                    tasks[0] = NetClient.FindText(texts[0], SearchOptions.EndsWith);
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        tasks[i] = NetClient.FindText(texts[i], SearchOptions.Exact);
                    }
                    tasks[tasks.Length - 1] = NetClient.FindText(texts[tasks.Length - 1], SearchOptions.StartsWith);
                    Task.WaitAll(tasks);
                    if (tasks[0].Result != null)
                    {
                        if (Program.DebugLevel > 0)
                        {
                            foreach (var pp in tasks[0].Result.Hits)
                            {
                                Console.WriteLine("#{0}", pp.Path);
                                foreach (var position in pp.Positions)
                                {
                                    Console.WriteLine("#{0,6} {1}", position, texts[0]);
                                }
                            }
                        }
                        rangesList = PathRanges.ToList(tasks[0].Result.Hits);
                        for (int i = 1; i < tasks.Length; i++)
                        {
                            if (tasks[i].Result == null)
                            {
                                rangesList = null;
                                break;
                            }
                            if (Program.DebugLevel > 0)
                            {
                                foreach (var pp in tasks[i].Result.Hits)
                                {
                                    Console.WriteLine("#{0}", pp.Path);
                                    foreach (var position in pp.Positions)
                                    {
                                        Console.WriteLine("#{0,6} {1}", position, texts[i]);
                                    }
                                }
                            }
                            rangesList = PathRanges.Merge(rangesList, tasks[i].Result.Hits);
                        }
                    }
                }
                return rangesList != null ? SearchResult.ToPathLines(rangesList) : new PathLines[0];
            }
        }

        public bool UpdateIndex(string group, string path)
        {
            NetClient.GroupName = group;
            using (var sr = new StreamReader(path, true))
            {
                var tokenizer = new Tokenizer();
                tokenizer.Run(sr);
                if (Program.DebugLevel > 0)
                {
                    Console.WriteLine("#{0}", path);
                    for (int index = 0; index < tokenizer.Texts.Count; index++)
                    {
                        Console.WriteLine("#{0,6} {1,6} {2}", tokenizer.Lines[index] + 1, index, tokenizer.Texts[index]);
                    }
                }
                var req = new UpdateIndexRequest();
                req.Path = path;
                req.Texts = tokenizer.Texts.ToArray();
                var task = NetClient.UpdateIndex(req);
                task.Wait();
                var rsp = task.Result;
                return rsp != null;
            }
        }

        public bool DeleteIndex(string group)
        {
            NetClient.GroupName = group;
            var task = NetClient.DeleteIndex();
            task.Wait();
            return task.Result;
        }
    }
}
