using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.view
{
    public partial class MainWindow : Window
    {
        private static readonly string STATE_PATH = System.IO.Path.Combine(AppData.DirectoryPath, "state.json");

        private readonly TextSearchClient client = new TextSearchClient();

        private bool CanStartQuery => GroupComboBox.SelectedItem != null && QueryTextBox.Text.Trim().Length > 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = client;
            QueryButton.IsEnabled = CanStartQuery;
            StatusBarLabel.Content = " ";
            Activated += OnFirstActivate;
            Closed += OnClosed;
        }

        private async void OnFirstActivate(object sender, EventArgs e)
        {
            Activated -= OnFirstActivate;
            (new GridViewColumnWidthAdjuster(HitListView, HitListViewTextColumn, HitListViewNameColumn, HitListViewLineColumn)).Adjust();
            (new GridViewColumnWidthAdjuster(FileListView, FileListViewPathColumn, FileListViewSizeColumn)).Adjust();
            (new GridViewColumnWidthAdjuster(ContentListView, ContentListViewTextColumn, ContentListViewLineColumn)).Adjust();
            SwitchToFileList();
            using (var wip = WorkInProgress.Create()
                .DisableControl(FileAuthMenuItem)
                .DisableControl(EditReloadGroupsMenuItem)
                .SetContentControl(StatusBarLabel)
                .SetContent(Properties.Resources.Initializing))
            {
                if (await client.Initialize())
                {
                    QueryButton.IsEnabled = CanStartQuery;
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.InitializationFailure);
                }
            }
            LoadLastState();
            GroupComboBox.SelectionChanged += OnGroupComboBoxSelectionChanged;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SaveLastState();
        }

        private async void LoadLastState()
        {
            if (File.Exists(STATE_PATH))
            {
                var state = JsonConvert.DeserializeObject<LastState>(File.ReadAllText(STATE_PATH));
                if (state.Group != null && GroupComboBox.HasItems && GroupComboBox.Items.Contains(state.Group))
                {
                    GroupComboBox.SelectedItem = state.Group;
                }
                if (await client.UpdateFiles())
                {
                    if (state.Path != null && FileListView.ItemsSource is ObservableCollection<FileItem> ff)
                    {
                        var f = ff.Where(x => x.Path == state.Path).FirstOrDefault();
                        if (f != null)
                        {
                            FileListView.SelectedItem = f;
                            FileListView.ScrollIntoView(f);
                        }
                    }
                }
            }
        }

        private void SaveLastState()
        {
            var state = new LastState()
            {
                Group = GroupComboBox.SelectedItem is string s ? s : null,
                Path = FileListView.SelectedItem is FileItem f ? f.Path : null
            };
            File.WriteAllText(STATE_PATH, JsonConvert.SerializeObject(state));
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void OnFileExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnFileAuth(object sender, RoutedEventArgs e)
        {
            var dialogBox = new AuthWindow()
            {
                Owner = this,
                Username = IndexApiClient.Credentials.Username,
                Password = IndexApiClient.Credentials.Password
            };
            var result = dialogBox.ShowDialog();
            if (result == true)
            {
                bool authenticated;
                using (var wip = WorkInProgress.Create()
                    .DisableControl(FileAuthMenuItem)
                    .SetContentControl(StatusBarLabel)
                    .SetContent(Properties.Resources.WaitingForResponse))
                {
                    authenticated = await client.Authenticate(dialogBox.Username, dialogBox.Password);
                }
                if (authenticated)
                {
                    MessageBox.Show(Properties.Resources.AuthenticateSuccess, Properties.Resources.AuthWindowTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    await Dispatcher.BeginInvoke(new EventHandler<RoutedEventArgs>(OnReloadGroups), this, new RoutedEventArgs());
                }
                else
                {
                    MessageBox.Show(Properties.Resources.AuthenticateFailure, Properties.Resources.AuthWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void OnReloadGroups(object sender, RoutedEventArgs e)
        {
            using (var wip = WorkInProgress.Create()
                .DisableControl(EditReloadGroupsMenuItem)
                .SetContentControl(StatusBarLabel)
                .SetContent(Properties.Resources.WaitingForResponse))
            {
                var last = GroupComboBox.SelectedItem;
                if (await client.UpdateGroups())
                {
                    if (last != null && GroupComboBox.HasItems && GroupComboBox.Items.Contains(last))
                    {
                        GroupComboBox.SelectedItem = last;
                    }
                    QueryButton.IsEnabled = CanStartQuery;
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.UpdateGroupsFailure);
                }
            }
        }

        private void OnViewClear(object sender, RoutedEventArgs e)
        {
            client.Clear();
            FileListView.SelectedItem = null;
            if (FileListView.Visibility == Visibility.Visible)
            {
                CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
            }
            StatusBarLabel.Content = " ";
        }

        private void OnHelpAbout(object sender, RoutedEventArgs e)
        {
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            var sb = new StringBuilder();
            sb.Append(versionInfo.ProductName);
            sb.AppendLine();
            sb.AppendFormat(Properties.Resources.VersionFormat, versionInfo.ProductVersion);
            sb.AppendLine();
            sb.Append(versionInfo.LegalCopyright);
            MessageBox.Show(sb.ToString(), Properties.Resources.AboutCaption, MessageBoxButton.OK);
        }

        private async void OnGroupComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await client.UpdateFiles();
        }

        private async void OnQueryStart(object sender, RoutedEventArgs e)
        {
            using (var wip = WorkInProgress.Create()
                .DisableControl(QueryButton)
                .SetContentControl(StatusBarLabel)
                .SetContent(Properties.Resources.WaitingForResponse))
            {
                SwitchToHitList();
                var message = await client.Execute();
                if (message == null)
                {
                    wip.SetFinalContent(Properties.Resources.HitFormat, client.HitItems.Count);
                }
                else
                {
                    MessageBox.Show(message, Properties.Resources.AppCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void OnQueryTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            QueryButton.IsEnabled = CanStartQuery;
        }

        private void OnQueryTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                Dispatcher.BeginInvoke(new EventHandler<RoutedEventArgs>(OnQueryStart), this, new RoutedEventArgs());
            }
        }

        private async void OnHitListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HitListView.SelectedItem is HitItem h)
            {
                await client.OnSelectionChanged(h);
                int v = h.Line - 1;
                ContentListView.SelectedIndex = v;
                ContentListView.ScrollIntoView(ContentListView.SelectedItem);
            }
            else
            {
                ContentListView.SelectedIndex = -1;
            }
        }

        private async void OnFileListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileListView.SelectedItem is FileItem f)
            {
                if (HitListView.SelectedItem == null || HitListView.SelectedItem is HitItem h && h.Fid != f.Fid)
                {
                    if (f.HitRows > 0 && HitListView.ItemsSource is ObservableCollection<HitItem> hh)
                    {
                        HitListView.SelectedItem = hh.Where(x => x.Fid == f.Fid).FirstOrDefault();
                    }
                    else
                    {
                        HitListView.SelectedItem = null;
                    }
                    if (HitListView.SelectedItem == null)
                    {
                        await client.OnSelectionChanged(f);
                        ContentListView.SelectedIndex = 0;
                        ContentListView.ScrollIntoView(ContentListView.SelectedItem);
                        ContentListView.SelectedIndex = -1;
                    }
                }
            }
        }

        private void OnUpperViewSwitchButtonClick(object sender, RoutedEventArgs e)
        {
            if (HitListView.Visibility == Visibility.Visible)
            {
                SwitchToFileList();
            }
            else if (FileListView.Visibility == Visibility.Visible)
            {
                SwitchToHitList();
            }
        }

        private void SwitchToHitList()
        {
            if (HitListView.Visibility == Visibility.Hidden)
            {
                HitListView.Visibility = Visibility.Visible;
                FileListView.Visibility = Visibility.Hidden;
                if (HitListView.ItemsSource is ObservableCollection<HitItem> hh)
                {
                    if (FileListView.SelectedItem is FileItem f)
                    {
                        if (HitListView.SelectedItem == null || HitListView.SelectedItem is HitItem h && h.Fid != f.Fid)
                        {
                            HitListView.SelectedItem = hh.Where(x => x.Fid == f.Fid).FirstOrDefault();
                        }
                    }
                }
                if (HitListView.SelectedItem != null)
                {
                    HitListView.ScrollIntoView(HitListView.SelectedItem);
                }
                UpperViewSwitchButton.Content = Properties.Resources.SwitchToFileList;
                UpperViewSwitchButton.ToolTip = Properties.Resources.SwitchToFileListTooltip;
            }
        }

        private void SwitchToFileList()
        {
            if (FileListView.Visibility == Visibility.Hidden)
            {
                HitListView.Visibility = Visibility.Hidden;
                FileListView.Visibility = Visibility.Visible;
                CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
                if (FileListView.ItemsSource is ObservableCollection<FileItem> ff)
                {
                    if (HitListView.SelectedItem is HitItem h)
                    {
                        FileListView.SelectedItem = ff.Where(x => x.Fid == h.Fid).FirstOrDefault();
                    }
                }
                if (FileListView.SelectedItem != null)
                {
                    FileListView.ScrollIntoView(FileListView.SelectedItem);
                }
                UpperViewSwitchButton.Content = Properties.Resources.SwitchToHitList;
                UpperViewSwitchButton.ToolTip = Properties.Resources.SwitchToHitListTooltip;
            }
        }
    }
}
