using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                var tokenizer = new TextTokenizer();
                tokenizer.Run(sr);
                var qTexts = tokenizer.Texts;
                DebugPut("phrase", text, qTexts);
                if (tokenizer.Tokens.Count == 1)
                {
                    var client = new IndexApiClient();
                    var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.Contains);
                    task.Wait();
                    if (task.Result == null)
                    {
                        throw NewResponseException(client.Response);
                    }
                    rangesList = HitRangesListExtension.ToList(task.Result);
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
                    rangesList = HitRangesListExtension.ToList(tasks[0].Result);
                    for (int i = 1; i < tasks.Length; i++)
                    {
                        if (tasks[i].Result == null)
                        {
                            throw NewResponseException(clients[i].Response);
                        }
                        rangesList.Merge(tasks[i].Result);
                    }
                }
                if (rangesList.Count > 0)
                {
                    var tasks = new Task<HitRowColumns>[rangesList.Count];
                    for (int index = 0; index < rangesList.Count; index++)
                    {
                        var entry = rangesList[index];
                        tasks[index] = ToHitRowColumns(qTexts, entry.Fid, entry.Ranges);
                    }
                    var list = new List<HitRowColumns>();
                    Task.WaitAll(tasks);
                    foreach (var task in tasks)
                    {
                        if (task.Result.Rows.Count > 0)
                        {
                            list.Add(task.Result);
                        }
                    }
                    return list.ToArray();
                }
                else
                {
                    return new HitRowColumns[0];
                }
            }
        }

        private static async Task<HitRowColumns> ToHitRowColumns(string[] qTexts, int fid, List<(int Start, int End)> ranges)
        {
            var dct = new Dictionary<int, List<(int Start, int End)>>();
            try
            {
                var client = new IndexApiClient();
                var contents = await client.DownloadFile(fid);
                var tokenizer = new TextTokenizer();
                tokenizer.Run(contents.Lines);
                foreach (var range in ranges)
                {
                    if (range.End < tokenizer.Tokens.Count)
                    {
                        if (tokenizer.Tokens[range.Start].Row == tokenizer.Tokens[range.End].Row)
                        {
                            if (!dct.TryGetValue(tokenizer.Tokens[range.Start].Row, out var colRanges))
                            {
                                colRanges = new List<(int Start, int End)>();
                                dct.Add(tokenizer.Tokens[range.Start].Row, colRanges);
                            }
                            if (qTexts.Length == 1)
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
                            else // if (qTexts.Length > 1)
                            {
                                int start = tokenizer.Tokens[range.Start].Column + tokenizer.Tokens[range.Start].Text.Length - qTexts.First().Length;
                                int end = tokenizer.Tokens[range.End].Column + qTexts.Last().Length;
                                colRanges.Add((start, end));
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
            return new HitRowColumns(fid, list);
        }
    }
}
