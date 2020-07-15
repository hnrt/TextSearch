using com.hideakin.textsearch.service;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        private HitItem CurrentSelection { get; set; }

        public ObservableCollection<FileItem> FileItems { get; } = new ObservableCollection<FileItem>();

        public FileItem CurrentFileItem { get; set; }

        public string Path { get; private set; } = " ";

        public ObservableCollection<LineText> Contents { get; } = new ObservableCollection<LineText>();

        private IndexService IndexSvc { get; } = new IndexService();

        public TextSearchClient()
        {
        }

        public async Task<bool> Initialize()
        {
            var UpdateGroupsResult = await UpdateGroups();
            return UpdateGroupsResult;
        }

        public async Task<bool> Authenticate(string username, string password)
        {
            var api = IndexApiClient.Create();
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
            var api = IndexApiClient.Create();
            var results = await api.GetFileGroups();
            if (results != null)
            {
                string defaultGroup = "default";
                Groups.Add(defaultGroup);
                foreach (var group in results.OrderBy(x => x.Name))
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
                var api = IndexApiClient.Create();
                var results = await api.GetFiles(Group);
                if (results != null)
                {
                    foreach (var entry in results.OrderBy(x => x.Path.ToUpperInvariant()))
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
            HitItems.Clear();
            Path = string.Empty;
            NotifyOfChange("Path");
            Contents.Clear();
            var api = IndexApiClient.Create();
            var results = await Task<object>.Run(() =>
            {
                try
                {
                    return (object)IndexSvc.FindText(Group, QueryText);
                }
                catch (Exception e)
                {
                    return e;
                }
            });
            if (results is HitRowColumns[] hits)
            {
                foreach (var hit in hits)
                {
                    var contents = FileContents.Find(hit.Fid);
                    foreach (var entry in hit.Rows)
                    {
                        HitItems.Add(new HitItem(hit.Fid, contents.Path, entry.Row + 1, contents.Lines[entry.Row], entry.Columns));
                    }
                }
                var d = new Dictionary<int, int>();
                foreach (var h in HitItems)
                {
                    if (d.TryGetValue(h.Fid, out var n))
                    {
                        d[h.Fid] = n + 1;
                    }
                    else
                    {
                        d.Add(h.Fid, 1);
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
            else if (results is Exception e)
            {
                return e.Message;
            }
            return null;
        }

        public bool OnSelectionChanged(HitItem h)
        {
            bool contentsChanged = false;
            if (h != null && (CurrentSelection == null || CurrentSelection.Fid != h.Fid))
            {
                Path = h.Path;
                NotifyOfChange("Path");
                Contents.Clear();
                var contents = FileContents.Find(h.Fid);
                int no = 1;
                foreach (var line in contents.Lines)
                {
                    Contents.Add(new LineText(no, line, null));
                    no++;
                }
                foreach (var item in HitItems)
                {
                    if (item.Fid == h.Fid)
                    {
                        Contents[item.Line - 1].Matches = item.Matches;
                    }
                }
                contentsChanged = true;
            }
            CurrentSelection = h;
            return contentsChanged;
        }

        public async Task<bool> OnFileSelectionChanged(FileItem f)
        {
            bool contentsChanged = false;
            if (f != null)
            {
                if (CurrentFileItem == null || CurrentFileItem.Fid != f.Fid)
                {
                    Path = f.Path;
                    NotifyOfChange("Path");
                    Contents.Clear();
                    var contents = FileContents.Find(f.Fid);
                    if (contents == null)
                    {
                        var api = IndexApiClient.Create();
                        contents = await api.DownloadFile(f.Fid);
                    }
                    if (contents != null)
                    {
                        int no = 1;
                        foreach (var line in contents.Lines)
                        {
                            Contents.Add(new LineText(no, line, null));
                            no++;
                        }
                        if (HitItems.Count > 0)
                        {
                            foreach (var item in HitItems)
                            {
                                if (item.Fid == f.Fid)
                                {
                                    Contents[item.Line - 1].Matches = item.Matches;
                                }
                            }
                        }
                    }
                    contentsChanged = true;
                }
            }
            else if (CurrentFileItem != null)
            {
                Path = " ";
                NotifyOfChange("Path");
                Contents.Clear();
            }
            CurrentFileItem = f;
            return contentsChanged;
        }

        public void Clear()
        {
            QueryText = string.Empty;
            NotifyOfChange("QueryText");
            HitItems.Clear();
            CurrentSelection = null;
            Path = " ";
            NotifyOfChange("Path");
            Contents.Clear();
        }

        private void NotifyOfChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
