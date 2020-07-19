using com.hideakin.textsearch.data;
using com.hideakin.textsearch.exception;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    public class IndexService : ServiceBase
    {
        public IndexService(CancellationToken ct)
            : base(ct)
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
                    using(var client = IndexApiClient.Create(ct))
                    {
                        var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.Contains);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            rangesList = HitRangesListExtension.ToList(hits);
                        }
                        else if (task.Result is Exception e)
                        {
                            throw e;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                else if (tokenizer.Tokens.Count > 1)
                {
                    var clients = new IndexApiClient[tokenizer.Tokens.Count];
                    var tasks = new Task<object>[tokenizer.Tokens.Count];
                    var client = IndexApiClient.Create(ct);
                    clients[0] = client;
                    tasks[0] = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.EndsWith);
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        client = IndexApiClient.Create(ct);
                        clients[i] = client;
                        tasks[i] = client.FindText(group, tokenizer.Tokens[i].Text, SearchOptions.Exact);
                    }
                    client = IndexApiClient.Create(ct);
                    clients[tasks.Length - 1] = client;
                    tasks[tasks.Length - 1] = client.FindText(group, tokenizer.Tokens[tasks.Length - 1].Text, SearchOptions.StartsWith);
                    Task.WaitAll(tasks);
                    foreach (var c in clients)
                    {
                        c.Dispose();
                    }
                    {
                        if (tasks[0].Result is TextDistribution[] hits)
                        {
                            rangesList = HitRangesListExtension.ToList(hits);
                        }
                        else if (tasks[0].Result is Exception e)
                        {
                            throw e;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    for (int i = 1; i < tasks.Length; i++)
                    {
                        if (tasks[i].Result is TextDistribution[] hits)
                        {
                            rangesList.Merge(hits);
                        }
                        else if (tasks[i].Result is Exception e)
                        {
                            throw e;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                if (rangesList.Count > 0)
                {
                    var tasks = new Task<HitRowColumns>[rangesList.Count];
                    for (int index = 0; index < rangesList.Count; index++)
                    {
                        var entry = rangesList[index];
                        tasks[index] = ToHitRowColumns(qTexts, entry.Fid, entry.Ranges, ct);
                    }
                    var list = new List<HitRowColumns>();
                    Task.WaitAll(tasks);
                    foreach (var task in tasks)
                    {
                        if (task.Result != null && task.Result.Rows.Count > 0)
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

        public async Task<HitRowColumns[]> FindTextAsync(string group, string text)
        {
            return await Task.Run(() =>
            {
                return FindTextV2(group, text);
            });
        }

        public HitRowColumns[] FindTextV2(string group, string text)
        {
            const int LIMIT = 65536;
            using (var sr = new StringReader(text))
            {
                List<HitRanges> rangesList = null;
                var tokenizer = new TextTokenizer();
                tokenizer.Run(sr);
                var qTexts = tokenizer.Texts;
                DebugPut("phrase", text, qTexts);
                if (tokenizer.Tokens.Count == 1)
                {
                    using (var client = IndexApiClient.Create(ct))
                    {
                        int offset = 0;
                        while (true)
                        {
                            var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.Contains, LIMIT, offset);
                            task.Wait();
                            if (task.Result is TextDistribution[] hits)
                            {
                                if (rangesList == null)
                                {
                                    rangesList = HitRangesListExtension.ToList(hits);
                                    if (hits.Length == 0)
                                    {
                                        break;
                                    }
                                }
                                else if (hits.Length == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    rangesList.Add(hits);
                                }
                            }
                            else if (task.Result is Exception e)
                            {
                                throw e;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            offset += LIMIT;
                        }
                    }
                }
                else if (tokenizer.Tokens.Count > 1)
                {
                    var clients = new IndexApiClient[tokenizer.Tokens.Count];
                    var tasks = new Task<object>[tokenizer.Tokens.Count];
                    var client = IndexApiClient.Create(ct);
                    clients[0] = client;
                    tasks[0] = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.EndsWith, LIMIT, 0);
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        client = IndexApiClient.Create(ct);
                        clients[i] = client;
                        tasks[i] = client.FindText(group, tokenizer.Tokens[i].Text, SearchOptions.Exact);
                    }
                    client = IndexApiClient.Create(ct);
                    clients[tasks.Length - 1] = client;
                    tasks[tasks.Length - 1] = client.FindText(group, tokenizer.Tokens[tasks.Length - 1].Text, SearchOptions.StartsWith, LIMIT, 0);
                    Task.WaitAll(tasks);
                    foreach (var c in clients)
                    {
                        c.Dispose();
                    }
                    {
                        if (tasks[0].Result is TextDistribution[] hits)
                        {
                            rangesList = HitRangesListExtension.ToList(hits);
                            if (hits.Length > 0)
                            {
                                using (client = IndexApiClient.Create(ct))
                                {
                                    int offset = LIMIT;
                                    while (true)
                                    {
                                        var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.EndsWith, LIMIT, offset);
                                        task.Wait();
                                        if (task.Result is TextDistribution[] hits2)
                                        {
                                            if (hits2.Length == 0)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                rangesList.Add(hits2);
                                            }
                                        }
                                        else if (task.Result is Exception e)
                                        {
                                            throw e;
                                        }
                                        else
                                        {
                                            throw new NotImplementedException();
                                        }
                                        offset += LIMIT;
                                    }
                                }
                            }
                        }
                        else if (tasks[0].Result is Exception e)
                        {
                            throw e;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    for (int i = 1; i < tasks.Length - 1; i++)
                    {
                        if (tasks[i].Result is TextDistribution[] hits)
                        {
                            rangesList.Merge(hits);
                        }
                        else if (tasks[i].Result is Exception e)
                        {
                            throw e;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    {
                        if (tasks[tasks.Length - 1].Result is TextDistribution[] hits)
                        {
                            rangesList.Merge(hits);
                            if (hits.Length > 0)
                            {
                                using (client = IndexApiClient.Create(ct))
                                {
                                    int offset = LIMIT;
                                    while (true)
                                    {
                                        var task = client.FindText(group, tokenizer.Tokens[tasks.Length - 1].Text, SearchOptions.StartsWith, LIMIT, offset);
                                        task.Wait();
                                        if (task.Result is TextDistribution[] hits2)
                                        {
                                            if (hits2.Length == 0)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                rangesList.Add(hits2);
                                            }
                                        }
                                        else if (task.Result is Exception e)
                                        {
                                            throw e;
                                        }
                                        else
                                        {
                                            throw new NotImplementedException();
                                        }
                                        offset += LIMIT;
                                    }
                                }
                            }
                        }
                        else if (tasks[tasks.Length - 1].Result is Exception e)
                        {
                            throw e;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                if (rangesList.Count > 0)
                {
                    var list = new List<HitRowColumns>();
                    var tasks = new Task<HitRowColumns>[8];
                    int taskCount = 0;
                    for (int index = 0; index < rangesList.Count; index++)
                    {
                        var entry = rangesList[index];
                        if (taskCount < tasks.Length)
                        {
                            tasks[taskCount++] = ToHitRowColumns(qTexts, entry.Fid, entry.Ranges, ct);
                        }
                        else
                        {
                            var completed = Task.WaitAny(tasks);
                            var task = tasks[completed];
                            if (completed < --taskCount)
                            {
                                tasks[completed] = tasks[taskCount];
                            }
                            tasks[taskCount++] = ToHitRowColumns(qTexts, entry.Fid, entry.Ranges, ct);
                            if (task.Result != null && task.Result.Rows.Count > 0)
                            {
                                list.Add(task.Result);
                            }
                        }
                    }
                    while (taskCount > 0)
                    {
                        var completed = Task.WaitAny(tasks);
                        var task = tasks[completed];
                        if (completed < --taskCount)
                        {
                            tasks[completed] = tasks[taskCount];
                        }
                        if (task.Result != null && task.Result.Rows.Count > 0)
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

        private static async Task<HitRowColumns> ToHitRowColumns(string[] qTexts, int fid, List<(int Start, int End)> ranges, CancellationToken ct)
        {
            using(var client = IndexApiClient.Create(ct))
            {
                var result = await client.DownloadFile(fid);
                if (result is FileContents contents)
                {
                    var dct = new Dictionary<int, List<(int Start, int End)>>();
                    var tokenizer = new TextTokenizer();
                    tokenizer.Run(contents.Lines);
                    foreach (var (start, end) in ranges)
                    {
                        if (end < tokenizer.Tokens.Count)
                        {
                            if (tokenizer.Tokens[start].Row == tokenizer.Tokens[end].Row)
                            {
                                if (!dct.TryGetValue(tokenizer.Tokens[start].Row, out var colRanges))
                                {
                                    colRanges = new List<(int Start, int End)>();
                                    dct.Add(tokenizer.Tokens[start].Row, colRanges);
                                }
                                if (qTexts.Length == 1)
                                {
                                    int index = tokenizer.Tokens[start].Text.IndexOf(qTexts[0]);
                                    while (index >= 0)
                                    {
                                        int start2 = tokenizer.Tokens[start].Column + index;
                                        int end2 = start2 + qTexts[0].Length;
                                        colRanges.Add((start2, end2));
                                        index = tokenizer.Tokens[start].Text.IndexOf(qTexts[0], index + qTexts[0].Length);
                                    }
                                }
                                else // if (qTexts.Length > 1)
                                {
                                    int start2 = tokenizer.Tokens[start].Column + tokenizer.Tokens[start].Text.Length - qTexts.First().Length;
                                    int end2 = tokenizer.Tokens[end].Column + qTexts.Last().Length;
                                    colRanges.Add((start2, end2));
                                }
                            }
                        }
                    }
                    if (dct.Count == 0)
                    {
                        return null;
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
                else if (result is FileNotExistException)
                {
                    return null;
                }
                else if (result is Exception e)
                {
                    throw e;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
