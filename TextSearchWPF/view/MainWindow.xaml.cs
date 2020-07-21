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
        #region FIELDS

        private static readonly string STATE_PATH = System.IO.Path.Combine(AppData.DirectoryPath, "state.json");

        private readonly TextSearchClient client = new TextSearchClient();

        private bool CanStartQuery => GroupComboBox.SelectedItem != null && QueryTextBox.Text.Trim().Length > 0;

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
        }

        #endregion

        #region CALLBACKS - WINDOW

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            EditCancelRequestMenuItem.IsEnabled = false;
            (new GridViewColumnWidthAdjuster(HitListView, HitListViewTextColumn, HitListViewNameColumn, HitListViewLineColumn)).Adjust();
            (new GridViewColumnWidthAdjuster(FileListView, FileListViewPathColumn, FileListViewSizeColumn)).Adjust();
            (new GridViewColumnWidthAdjuster(ContentListView, ContentListViewTextColumn, ContentListViewLineColumn)).Adjust();
            SwitchToFileList();
            UpdateUpperViewSwitchButton();
            using (var wip = RequestInProgress(Properties.Resources.Initializing))
            {
                if (await client.Initialize())
                {
                    QueryButton.IsEnabled = CanStartQuery;
                    wip.SetFinalContent(null);
                    UpdateStatusBar();
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
                using (var wip = RequestInProgress(Properties.Resources.Initializing))
                {
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
                        wip.SetFinalContent(null);
                        UpdateStatusBar();
                    }
                    else
                    {
                        wip.SetFinalContent(Properties.Resources.InitializationFailure);
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

        #endregion

        #region CALLBACKS - MENU

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

        private async void OnReloadGroups(object sender, RoutedEventArgs e)
        {
            using (var wip = RequestInProgress())
            {
                if (await client.UpdateGroups()) // may fire OnGroupComboBoxSelectionChanged if group selection is changed
                {
                    wip.SetFinalContent(null);
                    UpdateStatusBar();
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.UpdateGroupsFailure);
                }
                QueryButton.IsEnabled = CanStartQuery;
            }
        }

        private void OnEditCancelRequest(object sender, RoutedEventArgs e)
        {
            client.Cancel();
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

        #endregion

        #region CALLBACKS - GROUP COMBOBOX

        private async void OnGroupComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var wip = RequestInProgress())
            {
                if (await client.UpdateFiles())
                {
                    wip.SetFinalContent(null);
                    UpdateStatusBar();
                }
                else
                {
                    wip.SetFinalContent(Properties.Resources.UpdateFilesFailure);
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

        private void UpdateStatusBar(string message = null)
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
            return WorkInProgress.Create()
                    .DisableControl(FileAuthMenuItem)
                    .DisableControl(EditReloadGroupsMenuItem)
                    .EnableControl(EditCancelRequestMenuItem)
                    .DisableControl(QueryTextBox)
                    .DisableControl(QueryButton)
                    .SetContentControl(StatusBarLabel)
                    .SetContent(message ?? Properties.Resources.WaitingForResponse);
        }

        #endregion
    }
}
