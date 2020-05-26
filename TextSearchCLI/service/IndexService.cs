﻿using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;
using System.IO;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    internal class IndexService
    {
        private IndexNetClient netClient = IndexNetClient.Instance;

        public IndexService()
        {
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
                return SearchResult.ToPathLines(rsp.Hits);
            }
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

        public bool DeleteIndex(string group)
        {
            netClient.GroupName = group;
            var task = netClient.DeleteIndex();
            task.Wait();
            return task.Result;
        }
    }
}
