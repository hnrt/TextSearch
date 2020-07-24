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
    public class IndexService
    {
        private CancellationToken ct;

        private readonly Dictionary<string, Dictionary<int, List<int>>> textMap = new Dictionary<string, Dictionary<int, List<int>>>();

        public int ConcurrencyLevel { get; set; } = 8;

        public IndexService(CancellationToken ct)
        {
            this.ct = ct;
        }

        public async Task<HitRowColumns[]> FindTextAsync(string group, string text)
        {
            return await Task.Run(() =>
            {
                return FindText(group, text);
            });
        }

        public HitRowColumns[] FindText(string group, string text)
        {
            const int LIMIT = 65536;
            using (var sr = new StringReader(text))
            {
                List<HitRanges> rangesList = null;
                var tokenizer = new TextTokenizer();
                tokenizer.Run(sr);
                var qTexts = tokenizer.Texts;
                DebugPut("phrase", text, qTexts);
                var client = IndexApiClient.Create(ct);
                if (tokenizer.Tokens.Count == 1)
                {
                    {
                        var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.Contains, LIMIT);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0)
                            {
                                return new HitRowColumns[0];
                            }
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
                    for (int offset = LIMIT; true; offset += LIMIT)
                    {
                        var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.Contains, LIMIT, offset);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0)
                            {
                                break;
                            }
                            rangesList.Merge(hits);
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
                    {
                        var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.EndsWith, LIMIT);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0)
                            {
                                return new HitRowColumns[0];
                            }
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
                    for (int offset = LIMIT; true; offset += LIMIT)
                    {
                        var task = client.FindText(group, tokenizer.Tokens[0].Text, SearchOptions.EndsWith, LIMIT, offset);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0)
                            {
                                break;
                            }
                            rangesList.Merge(hits);
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
                    for (int index = 1; index < tokenizer.Tokens.Count - 1; index++)
                    {
                        var task = client.FindText(group, tokenizer.Tokens[index].Text, SearchOptions.Exact);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0 || rangesList.AddNext(hits).Count == 0)
                            {
                                return new HitRowColumns[0];
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
                    }
                    var last = new List<TextDistribution>();
                    {
                        var task = client.FindText(group, tokenizer.Tokens[tokenizer.Tokens.Count - 1].Text, SearchOptions.StartsWith, LIMIT);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0)
                            {
                                return new HitRowColumns[0];
                            }
                            last.AddRange(hits);
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
                    for (int offset = LIMIT; true; offset += LIMIT)
                    {
                        var task = client.FindText(group, tokenizer.Tokens[tokenizer.Tokens.Count - 1].Text, SearchOptions.StartsWith, LIMIT, offset);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0)
                            {
                                break;
                            }
                            last.Merge(hits);
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
                    if (rangesList.AddNext(last).Count == 0)
                    {
                        return new HitRowColumns[0];
                    }
                }
                else
                {
                    return new HitRowColumns[0];
                }
                var list = new List<HitRowColumns>();
                var tasks = new Task<HitRowColumns>[rangesList.Count < 4 ? rangesList.Count : 4];
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
                while (taskCount > 1)
                {
                    if (taskCount < tasks.Length)
                    {
                        var tasks2 = new Task<HitRowColumns>[taskCount];
                        Array.Copy(tasks, tasks2, taskCount);
                        tasks = tasks2;
                    }
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
                {
                    var task = tasks[0];
                    task.Wait();
                    if (task.Result != null && task.Result.Rows.Count > 0)
                    {
                        list.Add(task.Result);
                    }
                }
                return list.ToArray();
            }
        }

        public void StartIndexing()
        {
            textMap.Clear();
        }

        public void PopulateIndex(string path, int fid)
        {
            using (var input = new StreamReader(path, true))
            {
                var tokenizer = new TextTokenizer();
                tokenizer.Run(input);
                var texts = tokenizer.Texts;
                for (int index = 0; index < texts.Length; index++)
                {
                    var text = texts[index];
                    if (!textMap.TryGetValue(text, out var entry))
                    {
                        textMap.Add(text, new Dictionary<int, List<int>>() { { fid, new List<int>() { index } } });
                    }
                    else if (!entry.TryGetValue(fid, out var positions))
                    {
                        entry.Add(fid, new List<int>() { index });
                    }
                    else
                    {
                        positions.Add(index);
                    }
                }
            }
        }

        public void EndIndexing(string group, Action<string, int, int> callback = null)
        {
            var tasks = new List<(string Text, IndexApiClient Client, Task<object> IndexTask)>();
            var count = textMap.Count;
            var index = 0;
            foreach (var kvp in textMap)
            {
                var list = new List<TextDistribution>();
                foreach (var kvp2 in kvp.Value)
                {
                    list.Add(new TextDistribution()
                    {
                        Fid = kvp2.Key,
                        Positions = kvp2.Value.ToArray()
                    });
                }
                if (tasks.Count >= ConcurrencyLevel)
                {
                    var completed = Task.WaitAny(tasks.Select(x => x.IndexTask).ToArray());
                    var (text, client, task) = tasks[completed];
                    tasks.RemoveAt(completed);
                    if (task.Result == null)
                    {
                        callback?.Invoke(text, ++index, count);
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
                {
                    var text = kvp.Key;
                    var client = IndexApiClient.Create(ct);
                    var task = client.PostText(group, text, list.ToArray());
                    tasks.Add((text, client, task));
                }
            }
            while (tasks.Count > 0)
            {
                var completed = Task.WaitAny(tasks.Select(x => x.IndexTask).ToArray());
                var (text, client, task) = tasks[completed];
                tasks.RemoveAt(completed);
                if (task.Result == null)
                {
                    callback?.Invoke(text, ++index, count);
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

        private static async Task<HitRowColumns> ToHitRowColumns(string[] qTexts, int fid, List<(int Start, int End)> ranges, CancellationToken ct)
        {
            var contents = FileContents.Find(fid);
            if (contents == null)
            {
                var client = IndexApiClient.Create(ct);
                var result = await client.DownloadFile(fid);
                if (result is FileContents c)
                {
                    contents = c;
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

        private void DebugPut(string header, string input, string[] texts)
        {
            if (Debug.Level > 0)
            {
                if (header != null)
                {
                    Console.WriteLine("#{0}: {1}", header, input);
                }
                else
                {
                    Console.WriteLine("#{0}", input);
                }
                int index = 0;
                foreach (var t in texts)
                {
                    Console.WriteLine("#{0,6} {1}", index++, t);
                }
            }
        }
    }
}
