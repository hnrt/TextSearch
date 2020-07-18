using com.hideakin.textsearch.service;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.model
{
    internal class TextSearchClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Groups { get; } = new ObservableCollection<string>();

        public string Group { get; set; }

        public string QueryText { get; set; }

        public ObservableCollection<HitItem> HitItems { get; } = new ObservableCollection<HitItem>();

        public ObservableCollection<FileItem> FileItems { get; } = new ObservableCollection<FileItem>();

        private int Fid { get; set; } = -1;

        public string Path { get; private set; } = " ";

        public ObservableCollection<LineText> Contents { get; } = new ObservableCollection<LineText>();

        private IndexService IndexSvc { get; }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public TextSearchClient()
        {
            IndexSvc = new IndexService(cts.Token);
        }

        public void Cancel()
        {
            cts.Cancel();
        }

        public async Task<bool> Initialize()
        {
            var UpdateGroupsResult = await UpdateGroups();
            return UpdateGroupsResult;
        }

        public async Task<bool> Authenticate(string username, string password)
        {
            var api = IndexApiClient.Create(cts.Token);
            var result = await api.Authenticate(username, password);
            if (result is ErrorResponse e)
            {
                Console.WriteLine("{0}", e.ErrorDescription);
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateGroups()
        {
            string lastSelection = Group;
            Groups.Clear();
            var api = IndexApiClient.Create(cts.Token);
            var results = await api.GetFileGroups();
            if (results is FileGroupInfo[] groups)
            {
                string defaultGroup = "default";
                Groups.Add(defaultGroup);
                foreach (var group in groups.OrderBy(x => x.Name))
                {
                    if (group.Name != defaultGroup)
                    {
                        Groups.Add(group.Name);
                    }
                }
                Group = (lastSelection != null && Groups.Contains(lastSelection)) ? lastSelection : defaultGroup;
                NotifyOfChange("Group");
                return true;
            }
            else
            {
                if (Group != null)
                {
                    Group = null;
                    NotifyOfChange("Group");
                }
                return false;
            }
        }

        public async Task<bool> UpdateFiles()
        {
            FileItems.Clear();
            if (Group != null)
            {
                var api = IndexApiClient.Create(cts.Token);
                var results = await api.GetFiles(Group);
                if (results is FileInfo[] entries)
                {
                    foreach (var entry in entries.OrderBy(x => x.Path.ToUpperInvariant()))
                    {
                        FileItems.Add(new FileItem(entry.Fid, entry.Path, entry.Size));
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public async Task<string> Execute()
        {
            try
            {
                HitItems.Clear();
                Path = string.Empty;
                NotifyOfChange("Path");
                Contents.Clear();
                var hits = await IndexSvc.FindTextAsync(Group, QueryText);
                var m = new List<(HitRowColumns Hit, FileContents Contents)>();
                foreach (var hit in hits)
                {
                    var contents = await DownloadFile(hit.Fid);
                    m.Add((hit, contents));
                }
                m.Sort((a, b) =>
                {
                    return a.Contents.Path.ToUpperInvariant().CompareTo(b.Contents.Path.ToUpperInvariant());
                });
                var d = new Dictionary<int, int>();
                foreach (var (hit, contents) in m)
                {
                    foreach (var entry in hit.Rows)
                    {
                        var item = new HitItem(contents.Fid, contents.Path, entry.Row + 1, contents.Lines[entry.Row], entry.Columns);
                        HitItems.Add(item);
                        if (d.TryGetValue(item.Fid, out var hitRows))
                        {
                            d[item.Fid] = hitRows + 1;
                        }
                        else
                        {
                            d.Add(item.Fid, 1);
                        }
                    }
                }
                foreach (var f in FileItems)
                {
                    if (d.TryGetValue(f.Fid, out var hitRows))
                    {
                        f.HitRows = hitRows;
                    }
                    else
                    {
                        f.HitRows = 0;
                    }
                }
                return null;
            }
            catch (AggregateException ae)
            {
                var sb = new StringBuilder();
                foreach (var e in ae.InnerExceptions)
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }
                    sb.Append(e.Message);
                    Exception x = e;
                    while ((x = x.InnerException) != null)
                    {
                        sb.AppendLine();
                        sb.Append(x.Message);
                    }
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<bool> OnSelectionChanged(HitItem h)
        {
            if (h != null && h.Fid != Fid)
            {
                await SetContents(h.Fid, h.Path);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> OnSelectionChanged(FileItem f)
        {
            if (f != null && f.Fid != Fid)
            {
                await SetContents(f.Fid, f.Path);
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task SetContents(int fid, string path)
        {
            Fid = fid;
            Path = path;
            NotifyOfChange("Path");
            var contents = await DownloadFile(fid);
            Contents.Clear();
            if (contents != null)
            {
                int no = 1;
                foreach (var line in contents.Lines)
                {
                    Contents.Add(new LineText(no, line, null));
                    no++;
                }
                foreach (var item in HitItems)
                {
                    if (item.Fid == fid)
                    {
                        Contents[item.Line - 1].Matches = item.Matches;
                    }
                }
            }
        }

        private async Task<FileContents> DownloadFile(int fid)
        {
            var contents = FileContents.Find(fid);
            if (contents == null)
            {
                var api = IndexApiClient.Create(cts.Token);
                var result = await api.DownloadFile(fid);
                if (result is FileContents c)
                {
                    contents = c;
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
            return contents;
        }

        public void Clear()
        {
            QueryText = string.Empty;
            NotifyOfChange("QueryText");
            HitItems.Clear();
            Fid = -1;
            Path = " ";
            NotifyOfChange("Path");
            Contents.Clear();
            foreach (var f in FileItems)
            {
                f.HitRows = 0;
            }
            NotifyOfChange("FileItems");
        }

        private void NotifyOfChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
