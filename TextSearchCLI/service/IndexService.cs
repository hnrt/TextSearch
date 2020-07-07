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

        public HitRowColumns[] FindText(string group, string text)
        {
            using (var sr = new StringReader(text))
            {
                List<HitRanges> rangesList = null;
                var tokenizer = new Tokenizer();
                tokenizer.Run(sr);
                DebugPut("phrase", text, tokenizer.Texts);
                if (tokenizer.Tokens.Count == 1)
                {
                    var client = new IndexApiClient();
                    var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.Contains);
                    task.Wait();
                    if (task.Result == null)
                    {
                        throw NewResponseException(client.Response);
                    }
                    var array = task.Result;
                    rangesList = HitRanges.ToList(array);
                }
                else if (tokenizer.Tokens.Count > 1)
                {
                    var clients = new IndexApiClient[tokenizer.Tokens.Count];
                    var tasks = new Task<TextDistribution[]>[tokenizer.Tokens.Count];
                    var client = new IndexApiClient();
                    clients[0] = client;
                    tasks[0] = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.EndsWith);
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        client = new IndexApiClient();
                        clients[i] = client;
                        tasks[i] = client.FindText(group, tokenizer.Tokens[i].Text, SearchOptions.Exact);
                    }
                    client = new IndexApiClient();
                    clients[tasks.Length - 1] = client;
                    tasks[tasks.Length - 1] = client.FindText(group, tokenizer.Tokens[tasks.Length - 1].Text, SearchOptions.StartsWith);
                    Task.WaitAll(tasks);
                    if (tasks[0].Result == null)
                    {
                        throw NewResponseException(clients[0].Response);
                    }
                    var array = tasks[0].Result;
                    rangesList = HitRanges.ToList(array);
                    for (int i = 1; i < tasks.Length; i++)
                    {
                        if (tasks[i].Result == null)
                        {
                            throw NewResponseException(clients[i].Response);
                        }
                        array = tasks[i].Result;
                        rangesList = HitRanges.Merge(rangesList, array);
                    }
                }
                if (rangesList == null)
                {
                    return new HitRowColumns[0];
                }
                foreach (var ranges in rangesList)
                {
                    if (FileContents.Find(ranges.Fid) == null)
                    {
                        var client = new IndexApiClient();
                        var task = client.DownloadFile(ranges.Fid);
                        task.Wait();
                    }
                }
                return SearchResult.ToArrayOfPathRowColumns(tokenizer.Texts, rangesList);
            }
        }
    }
}
