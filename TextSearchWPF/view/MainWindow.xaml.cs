using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using Newtonsoft.Json;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.view
{
    public partial class MainWindow : Window
    {
        #region FIELDS

        private static readonly string STATE_PATH = System.IO.Path.Combine(AppData.DirectoryPath, "state.json");

        private readonly TextSearchClient client = new TextSearchClient();

        private bool CanStartQuery => GroupComboBox.SelectedItem != null && QueryTextBox.Text.Trim().Length > 0;

        private readonly List<string> Urls = new List<string>();

        #endregion

        #region CONSTRUCTOR

        public MainWindow()
        {
            InitializeComponent();
            DataContext = client;
            QueryButton.IsEnabled = CanStartQuery;
            StatusBarLabel.Content = " ";
            Loaded += OnLoaded;
            Closed += OnClosed;
            SizeChanged += OnWindowSizeChanged;
            statusBarDelay = new Delay<string>(ExecuteUpdateStatusBar);
        }

        #endregion

        #region CALLBACKS - WINDOW

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var state = LoadLastState();
            EditCancelRequestMenuItem.IsEnabled = false;
            (new GridViewColumnWidthAdjuster(HitListView, HitListViewTextColumn, HitListViewNameColumn, HitListViewLineColumn)).Adjust();
            (new GridViewColumnWidthAdjuster(FileListView, FileListViewPathColumn, FileListViewCheckColumn, FileListViewSizeColumn)).Adjust();
            (new GridViewColumnWidthAdjuster(ContentListView, ContentListViewTextColumn, ContentListViewLineColumn)).Adjust();
            SwitchToFileList();
            UpdateUpperViewSwitchButton();
            using (var wip = RequestInProgress(Properties.Resources.Initializing))
            {
                if (await client.Initialize())
                {
                    QueryButton.IsEnabled = CanStartQuery;
                    wip.SetFinalContent(Properties.Resources.InitializationSuccess);
                    UpdateStatusBar(new TimeSpan(0, 0, 10));
                    ApplyLastState(state, wip);
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.InitializationFailure);
                }
            }
            GroupComboBox.SelectionChanged += OnGroupComboBoxSelectionChanged;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SaveLastState();
        }

        private LastState LoadLastState()
        {
            if (File.Exists(STATE_PATH))
            {
                var state = JsonConvert.DeserializeObject<LastState>(File.ReadAllText(STATE_PATH));
                if (state.Url != null)
                {
                    IndexApiClient.Url = state.Url;
                }
                if (state.Urls != null)
                {
                    foreach (var url in state.Urls)
                    {
                        Urls.Add(url);
                    }
                }
                return state;
            }
            else
            {
                return null;
            }
        }

        private async void ApplyLastState(LastState state, WorkInProgress wip)
        {
            if (state != null)
            {
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
                    UpdateViewMenus();
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.InitializationFailure);
                }
            }
            UpdateStatusBar(new TimeSpan(0, 0, 10));
        }

        private void SaveLastState()
        {
            var state = new LastState()
            {
                Group = GroupComboBox.SelectedItem is string s ? s : null,
                Path = FileListView.SelectedItem is FileItem f ? f.Path : null,
                Url = IndexApiClient.Url
            };
            state.Urls = Urls.ToArray();
            File.WriteAllText(STATE_PATH, JsonConvert.SerializeObject(state));
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        #endregion

        #region CALLBACKS - MENU - FILE

        private void OnFileExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnFileUrl(object sender, RoutedEventArgs e)
        {
            var dialogBox = new UrlWindow()
            {
                Owner = this
            };
            dialogBox.Add(IndexApiClient.Url, true);
            foreach (var url in Urls.Where(x => x != IndexApiClient.Url))
            {
                dialogBox.Add(url);
            }
            var result = dialogBox.ShowDialog();
            if (result == true)
            {
                var url = dialogBox.Url;
                IndexApiClient.Url = url;
                if (!Urls.Contains(url))
                {
                    Urls.Add(url);
                }
            }
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
                using (var wip = RequestInProgress())
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

        #endregion

        #region CALLBACKS - MENU - EDIT

        private async void OnReloadGroups(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress())
            {
                if (await client.UpdateGroups()) // may fire OnGroupComboBoxSelectionChanged if group selection is changed
                {
                    if (wip.IsRoot)
                    {
                        wip.SetFinalContent(Properties.Resources.UpdateGroupsSuccess);
                    }
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.UpdateGroupsFailure);
                }
                QueryButton.IsEnabled = CanStartQuery;
                if (wip.IsRoot)
                {
                    UpdateStatusBar(new TimeSpan(0, 0, 10));
                }
            }
        }

        private void OnEditCancelRequest(object sender, RoutedEventArgs e)
        {
            client.Cancel();
        }

        #endregion

        #region CALLBACKS - MENU - VIEW

        private void UpdateViewMenus()
        {
            ViewCheckMenuItem.Items.Clear();
            ViewUncheckMenuItem.Items.Clear();
            var exts = client.GetExtensions();
            foreach (var ext in exts)
            {
                var menu = new MenuItem()
                {
                    Header = string.Format(Properties.Resources.FileExtFormat, ext)
                };
                menu.Click += OnViewCheckClick;
                ViewCheckMenuItem.Items.Add(menu);
                menu = new MenuItem()
                {
                    Header = string.Format(Properties.Resources.FileExtFormat, ext)
                };
                menu.Click += OnViewUncheckClick;
                ViewUncheckMenuItem.Items.Add(menu);
            }
        }

        private void OnViewCheckAllClick(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress(Properties.Resources.Processing))
            {
                if (sender is MenuItem menu)
                {
                    if (client.ChangeFileCheck(true) > 0)
                    {
                        CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
                    }
                }
                wip.SetFinalContent(null);
                UpdateStatusBar();
            }
        }

        private void OnViewUncheckAllClick(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress(Properties.Resources.Processing))
            {
                if (sender is MenuItem menu)
                {
                    if (client.ChangeFileCheck(false) > 0)
                    {
                        CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
                    }
                }
                wip.SetFinalContent(null);
                UpdateStatusBar();
            }
        }

        private void OnViewCheckClick(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress(Properties.Resources.Processing))
            {
                if (sender is MenuItem menu)
                {
                    if (menu.Header is string h)
                    {
                        var ext = ParseExtension(h);
                        if (client.ChangeFileCheckByExt(ext, true) > 0)
                        {
                            CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
                        }
                    }
                }
                wip.SetFinalContent(null);
                UpdateStatusBar();
            }
        }

        private void OnViewUncheckClick(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress(Properties.Resources.Processing))
            {
                if (sender is MenuItem menu)
                {
                    if (menu.Header is string h)
                    {
                        var ext = ParseExtension(h);
                        if (client.ChangeFileCheckByExt(ext, false) > 0)
                        {
                            CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
                        }
                    }
                }
                wip.SetFinalContent(null);
                UpdateStatusBar();
            }
        }

        private static string ParseExtension(string header)
        {
            var ph = "{0}";
            var fmt = Properties.Resources.FileExtFormat;
            var off = fmt.IndexOf(ph);
            return header.Substring(off, header.Length - (fmt.Length - ph.Length));
        }

        private void OnViewUncheckByNoHitClick(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress(Properties.Resources.Processing))
            {
                if (sender is MenuItem menu)
                {
                    if (client.UnsetFileCheckByHitRows() > 0)
                    {
                        CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
                    }
                }
                wip.SetFinalContent(null);
                UpdateStatusBar();
            }
        }

        private void OnViewClear(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress(Properties.Resources.Processing))
            {
                wip.SetFinalContent(null);
                client.Clear();
                FileListView.SelectedItem = null;
                if (FileListView.Visibility == Visibility.Visible)
                {
                    CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
                }
                UpdateStatusBar();
            }
        }

        #endregion

        #region CALLBACKS - MENU - HELP

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

        #endregion

        #region CALLBACKS - GROUP COMBOBOX

        private async void OnGroupComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var wip = RequestInProgress())
            {
                if (await client.UpdateFiles())
                {
                    UpdateViewMenus();
                    if (wip.IsRoot)
                    {
                        wip.SetFinalContent(null);
                        UpdateStatusBar();
                    }
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.UpdateFilesFailure);
                    if (wip.IsRoot)
                    {
                        UpdateStatusBar(new TimeSpan(0, 0, 10));
                    }
                }
            }
        }

        #endregion

        #region CALLBACKS - QUERY TEXTBOX

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

        #endregion 

        #region CALLBACKS - QUERY BUTTON

        private async void OnQueryStart(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress())
            {
                SwitchToHitList();
                UpdateUpperViewSwitchButton();
                var message = await client.Execute();
                if (message == null)
                {
                    wip.SetFinalContent(Properties.Resources.HitFormat, client.HitItems.Count);
                }
                else if (message.Length > 0)
                {
                    MessageBox.Show(message, Properties.Resources.AppCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        #endregion

        #region CALLBACKS - HIT LISTVIEW

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

        #endregion

        #region CALLBACKS - FILE LISTVIEW

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

        private void OnFileCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (sender is CheckBox cb)
                {
                    if (cb.DataContext is FileItem item)
                    {
                        client.SetFileCheckByDirectory(System.IO.Path.GetDirectoryName(item.Path), item.Check);
                    }
                }
            }
            client.OnFileCheckChanged();
            CollectionViewSource.GetDefaultView(FileListView.ItemsSource).Refresh();
        }

        #endregion

        #region CALLBACKS - LISTVIEW SWITCH BUTTON

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
            UpdateUpperViewSwitchButton();
            UpdateStatusBar();
        }

        private void UpdateUpperViewSwitchButton()
        {
            if (HitListView.Visibility == Visibility.Visible)
            {
                UpperViewSwitchButton.Content = Properties.Resources.SwitchToFileList;
                UpperViewSwitchButton.ToolTip = Properties.Resources.SwitchToFileListTooltip;
            }
            else if (FileListView.Visibility == Visibility.Visible)
            {
                UpperViewSwitchButton.Content = Properties.Resources.SwitchToHitList;
                UpperViewSwitchButton.ToolTip = Properties.Resources.SwitchToHitListTooltip;
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
            }
        }

        private void SwitchToFileList()
        {
            if (FileListView.Visibility == Visibility.Hidden)
            {
                FileListView.Visibility = Visibility.Visible;
                HitListView.Visibility = Visibility.Hidden;
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
            }
        }

        #endregion

        #region STATUS BAR

        private Delay<string> statusBarDelay;

        private void UpdateStatusBar(string message = null)
        {
            statusBarDelay.RunImmediately(message);
        }

        private void UpdateStatusBar(TimeSpan delay, string message = null)
        {
            statusBarDelay.Schedule(delay, message);
        }

        private void ExecuteUpdateStatusBar(string message)
        {
            if (message != null)
            {
                StatusBarLabel.Content = message;
            }
            else if (HitListView.Visibility == Visibility.Visible)
            {
                if (HitListView.ItemsSource is ObservableCollection<HitItem> hh && hh.Count > 0)
                {
                    StatusBarLabel.Content = string.Format(Properties.Resources.HitFormat, hh.Count);
                }
                else
                {
                    StatusBarLabel.Content = " ";
                }
            }
            else if (FileListView.Visibility == Visibility.Visible)
            {
                if (FileListView.ItemsSource is ObservableCollection<FileItem> ff && ff.Count > 0)
                {
                    StatusBarLabel.Content = string.Format(Properties.Resources.FileFormat, ff.Count);
                }
                else
                {
                    StatusBarLabel.Content = " ";
                }
            }
        }

        #endregion

        #region HELPERS

        private WorkInProgress RequestInProgress(string message = null)
        {
            var wip = WorkInProgress.Create()
                    .DisableControl(FileUrlMenuItem)
                    .DisableControl(FileAuthMenuItem)
                    .DisableControl(EditReloadGroupsMenuItem)
                    .EnableControl(EditCancelRequestMenuItem)
                    .DisableControl(ViewCheckMenuItem)
                    .DisableControl(ViewUncheckMenuItem)
                    .DisableControl(ViewClearMenuItem)
                    .DisableControl(QueryTextBox)
                    .DisableControl(QueryButton);
            if (!statusBarDelay.IsScheduled)
            {
                wip.SetContentControl(StatusBarLabel)
                    .SetContent(message ?? Properties.Resources.WaitingForResponse);
            }
            return wip;
        }

        #endregion
    }
}
