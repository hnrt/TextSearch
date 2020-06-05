using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    internal class IndexService : ServiceBase
    {
        public IndexService()
            : base()
        {
        }

        public PathLines[] FindText(string group, string text)
        {
            using (var sr = new StringReader(text))
            {
                List<PathRanges> rangesList = null;
                var tokenizer = new Tokenizer();
                tokenizer.Run(sr);
                var texts = tokenizer.Texts;
                DebugPut("phrase", text, texts);
                if (texts.Count == 1)
                {
                    var client = new IndexNetClient() { GroupName = group };
                    var task = client.FindText(texts[0], SearchOptions.Contains);
                    task.Wait();
                    if (task.Result == null)
                    {
                        throw NewResponseException(client.Response);
                    }
                    var ppArray = task.Result;
                    DebugPut(texts[0], ppArray);
                    rangesList = PathRanges.ToList(ppArray);
                }
                else if (texts.Count > 1)
                {
                    var clients = new IndexNetClient[texts.Count];
                    var tasks = new Task<PathPositions[]>[texts.Count];
                    var client = new IndexNetClient() { GroupName = group };
                    clients[0] = client;
                    tasks[0] = client.FindText(texts[0], SearchOptions.EndsWith);
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        client = new IndexNetClient() { GroupName = group };
                        clients[i] = client;
                        tasks[i] = client.FindText(texts[i], SearchOptions.Exact);
                    }
                    client = new IndexNetClient() { GroupName = group };
                    clients[tasks.Length - 1] = client;
                    tasks[tasks.Length - 1] = client.FindText(texts[tasks.Length - 1], SearchOptions.StartsWith);
                    Task.WaitAll(tasks);
                    if (tasks[0].Result == null)
                    {
                        throw NewResponseException(clients[0].Response);
                    }
                    var ppArray = tasks[0].Result;
                    DebugPut(texts[0], ppArray);
                    rangesList = PathRanges.ToList(ppArray);
                    for (int i = 1; i < tasks.Length; i++)
                    {
                        if (tasks[i].Result == null)
                        {
                            throw NewResponseException(clients[i].Response);
                        }
                        ppArray = tasks[i].Result;
                        DebugPut(texts[i], ppArray);
                        rangesList = PathRanges.Merge(rangesList, ppArray);
                    }
                }
                return rangesList != null ? SearchResult.ToPathLines(rangesList) : new PathLines[0];
            }
        }

        public void UpdateIndex(string group, string path)
        {
            using (var sr = new StreamReader(path, true))
            {
                var tokenizer = new Tokenizer();
                tokenizer.Run(sr);
                DebugPut(null, path, tokenizer.Texts);
                var req = new UpdateIndexRequest();
                req.Path = path;
                req.Texts = tokenizer.Texts.ToArray();
                var client = new IndexNetClient() { GroupName = group };
                var task = client.UpdateIndex(req);
                task.Wait();
                if (task.Result == null)
                {
                    throw NewResponseException(client.Response);
                }
            }
        }

        public void DeleteIndex(string group)
        {
            var client = new IndexNetClient() { GroupName = group };
            var task = client.DeleteIndex();
            task.Wait();
            if (!task.Result)
            {
                throw NewResponseException(client.Response);
            }
        }
    }
}
