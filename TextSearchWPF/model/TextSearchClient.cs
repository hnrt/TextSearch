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

        private List<HitFile> QueryResults { get; set; }

        private ISet<int> Unchecked { get; } = new HashSet<int>();

        public ObservableCollection<FileItem> FileItems { get; } = new ObservableCollection<FileItem>();

        private int Fid { get; set; } = -1;

        public string Path { get; private set; } = " ";

        public ObservableCollection<LineText> Contents { get; } = new ObservableCollection<LineText>();

        private CancellationTokenSource cts;

        public TextSearchClient()
        {
            cts = new CancellationTokenSource();
        }

        public void Cancel()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
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
                if (lastSelection != null && Groups.Contains(lastSelection))
                {
                    Group = lastSelection;
                    NotifyOfChange("Group");
                }
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
            QueryResults = null;
            HitItems.Clear();
            FileItems.Clear();
            if (Group != null)
            {
                var api = IndexApiClient.Create(cts.Token);
                var results = await api.GetFiles(Group);
                if (results is FileInfo[] entries)
                {
                    foreach (var entry in entries.OrderBy(x => x.Path.ToUpperInvariant()))
                    {
                        FileItems.Add(new FileItem(entry.Fid, entry.Path, entry.Size, !Unchecked.Contains(entry.Fid)));
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
                QueryResults = null;
                HitItems.Clear();
                Fid = -1;
                Path = " ";
                NotifyOfChange("Path");
                Contents.Clear();
                var svc = new IndexService(cts.Token);
                QueryResults = await svc.FindTextAsync(Group, QueryText);
                ProcessQueryResults();
                return null;
            }
            catch (TaskCanceledException)
            {
                return string.Empty;
            }
            catch (AggregateException e) when(e.InnerExceptions[0] is TaskCanceledException)
            {
                return string.Empty;
            }
            catch (AggregateException e)
            {
                var sb = new StringBuilder();
                foreach (var x in e.InnerExceptions)
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }
                    sb.Append(x.Message);
                    Exception y = x;
                    while ((y = y.InnerException) != null)
                    {
                        sb.AppendLine();
                        sb.Append(y.Message);
#if DEBUG
                        sb.AppendFormat(" {0}", y.GetType().FullName);
                        if (y is System.Net.Sockets.SocketException s)
                        {
                            sb.AppendFormat(" SocketErrorCode={0}", s.SocketErrorCode.ToString());
                        }
#endif
                    }
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private async void ProcessQueryResults()
        {
            var m = new List<(HitFile Hit, FileContents Contents)>();
            foreach (var hit in QueryResults)
            {
                var f = FileItems.Where(x => x.Fid == hit.Fid).FirstOrDefault();
                if (f != null && f.Check)
                {
                    var contents = await DownloadFile(hit.Fid);
                    m.Add((hit, contents));
                }
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
                    var item = new HitItem(contents.Fid, contents.Path, entry.Row + 1, contents.Lines[entry.Row], entry.Matches.Select(x => (x.StartCol, x.EndCol)).ToList());
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

        public void OnFileCheckChanged()
        {
            foreach (var f in FileItems)
            {
                if (f.Check)
                {
                    if (Unchecked.Contains(f.Fid))
                    {
                        Unchecked.Remove(f.Fid);
                    }
                }
                else if (!Unchecked.Contains(f.Fid))
                {
                    Unchecked.Add(f.Fid);
                }
            }
            if (QueryResults == null || QueryResults.Count == 0)
            {
                return;
            }
            HitItems.Clear();
            ProcessQueryResults();
        }

        public void SetFileCheckByDirectory(string dirPath, bool value)
        {
            foreach (var x in FileItems)
            {
                var y = x.Path;
                if (y.StartsWith(dirPath) && (y[dirPath.Length] == '\\' || y[dirPath.Length] == '/'))
                {
                    x.Check = value;
                }
            }
        }

        public int UnsetFileCheckByHitRows()
        {
            int count = 0;
            foreach (var x in FileItems)
            {
                if (x.HitRows == 0 && x.Check)
                {
                    x.Check = false;
                    count++;
                }
            }
            return count;
        }

        public int ChangeFileCheck(bool value)
        {
            int changed = 0;
            foreach (var f in FileItems)
            {
                if (f.Check ^ value)
                {
                    changed++;
                }
                f.Check = value;
            }
            if (changed > 0)
            {
                OnFileCheckChanged();
            }
            return changed;
        }

        public int ChangeFileCheckByExt(string ext, bool value)
        {
            int changed = 0;
            foreach (var f in FileItems)
            {
                if (f.Path.ToLowerInvariant().EndsWith(ext))
                {
                    if (f.Check ^ value)
                    {
                        changed++;
                    }
                    f.Check = value;
                }
            }
            if (changed > 0)
            {
                OnFileCheckChanged();
            }
            return changed;
        }

        public string[] GetExtensions()
        {
            var exts = new List<string>();
            foreach (var f in FileItems)
            {
                int pos = f.Path.LastIndexOf(".");
                if (pos < 0)
                {
                    continue;
                }
                if (f.Path.IndexOfAny(new char[] { '\\', '/' }, pos) > pos)
                {
                    continue;
                }
                var ext = f.Path.Substring(pos).ToLowerInvariant();
                if (!exts.Contains(ext))
                {
                    exts.Add(ext);
                }
            }
            exts.Sort((x, y) => x.CompareTo(y));
            return exts.ToArray();
        }

        public void Clear()
        {
            QueryText = string.Empty;
            NotifyOfChange("QueryText");
            QueryResults = null;
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
