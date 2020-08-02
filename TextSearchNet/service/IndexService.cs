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

        #region USE INDEX

        public async Task<List<HitFile>> FindTextAsync(string group, string text)
        {
            return await Task.Run(() =>
            {
                return FindText(group, text);
            });
        }

        public List<HitFile> FindText(string group, string text)
        {
            return QueryExpression.Parse(text).Evaluate(FindTextSequence, group);
        }

        public List<HitFile> FindTextSequence(string group, string[] sequence, SearchOptions head, SearchOptions tail)
        {
            const int LIMIT = 65536;
            List<HitRanges> rangesList;
            var client = IndexApiClient.Create(ct);
            if (sequence.Length == 1)
            {
                if (head == SearchOptions.Exact && tail == SearchOptions.Exact)
                {
                    var task = client.FindText(group, sequence[0], SearchOptions.Exact);
                    task.Wait();
                    if (task.Result is TextDistribution[] hits)
                    {
                        if (hits.Length == 0)
                        {
                            return new List<HitFile>();
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
                else
                {
                    var option = head == SearchOptions.EndsWith && tail == SearchOptions.StartsWith ? SearchOptions.Contains :
                        head == SearchOptions.EndsWith && tail == SearchOptions.Exact ? SearchOptions.EndsWith :
                        head == SearchOptions.Exact && tail == SearchOptions.StartsWith ? SearchOptions.StartsWith :
                        throw new NotImplementedException();
                    {
                        var task = client.FindText(group, sequence[0], option, LIMIT);
                        task.Wait();
                        if (task.Result is TextDistribution[] hits)
                        {
                            if (hits.Length == 0)
                            {
                                return new List<HitFile>();
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
                        var task = client.FindText(group, sequence[0], option, LIMIT, offset);
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
            }
            else if (sequence.Length > 1)
            {
                {
                    var task = client.FindText(group, sequence[0], head, LIMIT);
                    task.Wait();
                    if (task.Result is TextDistribution[] hits)
                    {
                        if (hits.Length == 0)
                        {
                            return new List<HitFile>();
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
                    var task = client.FindText(group, sequence[0], head, LIMIT, offset);
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
                for (int index = 1; index < sequence.Length - 1; index++)
                {
                    var task = client.FindText(group, sequence[index], SearchOptions.Exact);
                    task.Wait();
                    if (task.Result is TextDistribution[] hits)
                    {
                        if (hits.Length == 0 || rangesList.Append(hits).Count == 0)
                        {
                            return new List<HitFile>();
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
                    var task = client.FindText(group, sequence[sequence.Length - 1], tail, LIMIT);
                    task.Wait();
                    if (task.Result is TextDistribution[] hits)
                    {
                        if (hits.Length == 0)
                        {
                            return new List<HitFile>();
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
                    var task = client.FindText(group, sequence[sequence.Length - 1], tail, LIMIT, offset);
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
                rangesList.Append(last);
            }
            else
            {
                rangesList = new List<HitRanges>();
            }
            return ToHitFileList(sequence, head, tail, rangesList);
        }

        public List<HitFile> ToHitFileList(string[] sequence, SearchOptions head, SearchOptions tail, List<HitRanges> rangesList)
        {
            var list = new List<HitFile>();
            var tasks = new List<Task<HitFile>>(ConcurrencyLevel);
            for (int index = 0; index < rangesList.Count; index++)
            {
                var entry = rangesList[index];
                if (tasks.Count < ConcurrencyLevel)
                {
                    tasks.Add(ToHitRowColumns(sequence, head, tail, entry.Fid, entry.Ranges, ct));
                }
                else
                {
                    var completed = Task.WaitAny(tasks.ToArray());
                    var task = tasks[completed];
                    tasks.RemoveAt(completed);
                    tasks.Add(ToHitRowColumns(sequence, head, tail, entry.Fid, entry.Ranges, ct));
                    if (task.Result != null && task.Result.Rows.Count > 0)
                    {
                        list.Add(task.Result);
                    }
                }
            }
            while (tasks.Count > 0)
            {
                var completed = Task.WaitAny(tasks.ToArray());
                var task = tasks[completed];
                tasks.RemoveAt(completed);
                if (task.Result != null && task.Result.Rows.Count > 0)
                {
                    list.Add(task.Result);
                }
            }
            return list;
        }

        private static async Task<HitFile> ToHitRowColumns(string[] sequence, SearchOptions head, SearchOptions tail, int fid, List<(int Start, int End)> ranges, CancellationToken ct)
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
            var dct = new Dictionary<int, MatchList>();
            var tokenizer = new TextTokenizer();
            tokenizer.Run(contents.Lines);
            foreach (var (startPos, endPos) in ranges)
            {
                if (endPos < tokenizer.Tokens.Count)
                {
                    if (tokenizer.Tokens[startPos].Row == tokenizer.Tokens[endPos].Row)
                    {
                        if (!dct.TryGetValue(tokenizer.Tokens[startPos].Row, out var matches))
                        {
                            matches = new MatchList();
                            dct.Add(tokenizer.Tokens[startPos].Row, matches);
                        }
                        if (sequence.Length == 1)
                        {
                            if (head == SearchOptions.EndsWith && tail == SearchOptions.StartsWith)
                            {
                                // i.e. SearchOptions.Contains - try to pick up all matches...
                                int index = tokenizer.Tokens[startPos].Text.IndexOf(sequence[0]);
                                while (index >= 0)
                                {
                                    int startCol = tokenizer.Tokens[startPos].Column + index;
                                    int endCol = startCol + sequence[0].Length;
                                    matches.Add((startPos, endPos, startCol, endCol));
                                    index = tokenizer.Tokens[startPos].Text.IndexOf(sequence[0], index + sequence[0].Length);
                                }
                            }
                            else if (head == SearchOptions.EndsWith && tail == SearchOptions.Exact)
                            {
                                int endCol = tokenizer.Tokens[startPos].Column + tokenizer.Tokens[startPos].Text.Length;
                                int startCol = endCol - sequence[0].Length;
                                matches.Add((startPos, endPos, startCol, endCol));
                            }
                            else //if (head == SearchOptions.Exact && tail == SearchOptions.StartsWith)
                            {
                                int startCol = tokenizer.Tokens[startPos].Column;
                                int endCol = tokenizer.Tokens[startPos].Column + sequence[0].Length;
                                matches.Add((startPos, endPos, startCol, endCol));
                            }
                        }
                        else // if (sequence.Length > 1)
                        {
                            int startCol =
                                head == SearchOptions.EndsWith ?
                                tokenizer.Tokens[startPos].Column + tokenizer.Tokens[startPos].Text.Length - sequence[0].Length :
                                tokenizer.Tokens[startPos].Column;
                            int endCol = tokenizer.Tokens[endPos].Column + sequence[sequence.Length - 1].Length;
                            matches.Add((startPos, endPos, startCol, endCol));
                        }
                    }
                }
            }
            if (dct.Count == 0)
            {
                return null;
            }
            return new HitFile(fid, dct.Select(x => new RowColumns(x.Key, x.Value)).OrderBy(x => x.Row).ToList());
        }

        #endregion

        #region CREATE INDEX

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

        #endregion
    }
}
