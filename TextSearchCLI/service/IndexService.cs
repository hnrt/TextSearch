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

        public PathRowColumns[] FindText(string group, string text)
        {
            using (var sr = new StringReader(text))
            {
                List<PathRanges> rangesList = null;
                var tokenizer = new Tokenizer();
                tokenizer.Run(sr);
                DebugPut("phrase", text, tokenizer.Texts);
                if (tokenizer.Tokens.Count == 1)
                {
                    var client = new IndexNetClient() { GroupName = group };
                    var task = client.FindText(tokenizer.Tokens[0].Text, SearchOptions.Contains);
                    task.Wait();
                    if (task.Result == null)
                    {
                        throw NewResponseException(client.Response);
                    }
                    var ppArray = task.Result;
                    DebugPut(tokenizer.Tokens[0].Text, ppArray);
                    rangesList = PathRanges.ToList(ppArray);
                }
                else if (tokenizer.Tokens.Count > 1)
                {
                    var clients = new IndexNetClient[tokenizer.Tokens.Count];
                    var tasks = new Task<PathPositions[]>[tokenizer.Tokens.Count];
                    var client = new IndexNetClient() { GroupName = group };
                    clients[0] = client;
                    tasks[0] = client.FindText(tokenizer.Tokens[0].Text, SearchOptions.EndsWith);
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        client = new IndexNetClient() { GroupName = group };
                        clients[i] = client;
                        tasks[i] = client.FindText(tokenizer.Tokens[i].Text, SearchOptions.Exact);
                    }
                    client = new IndexNetClient() { GroupName = group };
                    clients[tasks.Length - 1] = client;
                    tasks[tasks.Length - 1] = client.FindText(tokenizer.Tokens[tasks.Length - 1].Text, SearchOptions.StartsWith);
                    Task.WaitAll(tasks);
                    if (tasks[0].Result == null)
                    {
                        throw NewResponseException(clients[0].Response);
                    }
                    var ppArray = tasks[0].Result;
                    DebugPut(tokenizer.Tokens[0].Text, ppArray);
                    rangesList = PathRanges.ToList(ppArray);
                    for (int i = 1; i < tasks.Length; i++)
                    {
                        if (tasks[i].Result == null)
                        {
                            throw NewResponseException(clients[i].Response);
                        }
                        ppArray = tasks[i].Result;
                        DebugPut(tokenizer.Tokens[i].Text, ppArray);
                        rangesList = PathRanges.Merge(rangesList, ppArray);
                    }
                }
                return rangesList != null ? SearchResult.ToArrayOfPathRowColumns(tokenizer.Texts, rangesList) : new PathRowColumns[0];
            }
        }

        public void UpdateIndex(string group, string path)
        {
            using (var sr = new StreamReader(path, true))
            {
                var tokenizer = new Tokenizer();
                tokenizer.Run(sr);
                var texts = tokenizer.Texts;
                DebugPut(null, path, texts);
                var req = new UpdateIndexRequest();
                req.Path = path;
                req.Texts = texts.ToArray();
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
