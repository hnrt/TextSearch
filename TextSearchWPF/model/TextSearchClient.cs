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

        public string Path { get; private set; } = " ";

        public ObservableCollection<LineText> Contents { get; } = new ObservableCollection<LineText>();

        private IndexService IndexSvc { get; } = new IndexService();

        public TextSearchClient()
        {
        }

        public async Task Initialize()
        {
            await UpdateGroups();
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

        public async Task<string> Execute()
        {
            HitItems.Clear();
            NotifyOfChange("HitItems");
            Path = " ";
            NotifyOfChange("Path");
            Contents.Clear();
            NotifyOfChange("Contents");
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
                        HitItems.Add(new HitItem(hit.Fid, contents.Path, entry.Row + 1, contents.Lines[entry.Row]));
                    }
                }
                NotifyOfChange("HitItems");
            }
            else if (results is Exception e)
            {
                return e.Message;
            }
            return null;
        }

        public void OnSelectionChanged(HitItem h)
        {
            if (h == null)
            {
                return;
            }
            if (CurrentSelection == null || CurrentSelection.Fid != h.Fid)
            {
                Path = h.Path;
                NotifyOfChange("Path");
                Contents.Clear();
                var contents = FileContents.Find(h.Fid);
                int no = 1;
                foreach (var line in contents.Lines)
                {
                    Contents.Add(new LineText(no, line));
                    no++;
                }
                NotifyOfChange("Contents");
            }
            CurrentSelection = h;
        }

        private async Task UpdateGroups()
        {
            Groups.Clear();
            var api = IndexApiClient.Create();
            var results = await api.GetFileGroups();
            if (results != null)
            {
                foreach (var group in results)
                {
                    Groups.Add(group.Name);
                }
                NotifyOfChange("Groups");
                if (Group == null || !Groups.Contains(Group))
                {
                    Group = Groups.FirstOrDefault();
                    NotifyOfChange("Group");
                }
            }
            else
            {
                Group = null;
                NotifyOfChange("Group");
                NotifyOfChange("Groups");
            }
        }

        private void NotifyOfChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
