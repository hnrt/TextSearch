using System;
using System.Collections.ObjectModel;
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
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.view
{
    public partial class MainWindow : Window
    {
        private readonly TextSearchClient client = new TextSearchClient();

        private const double LISTVIEW_COLUMN_WIDTH_ADJUSTMENT = 6.0;
        private double HitListViewScrollContentPresenterWidth = double.NaN;
        private double FileListViewScrollContentPresenterWidth = double.NaN;
        private double ContentListViewScrollContentPresenterWidth = double.NaN;

        private bool CanStartQuery => GroupComboBox.SelectedItem != null && QueryTextBox.Text.Trim().Length > 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = client;
            QueryButton.IsEnabled = CanStartQuery;
            StatusBarLabel.Content = " ";
            Activated += OnFirstActivate;
        }

        private async void OnFirstActivate(object sender, EventArgs e)
        {
            Activated -= OnFirstActivate;
            HitListView.GetScrollContentPresenter().SizeChanged += OnHitListViewScrollContentPresenterSizeChanged;
            FileListView.GetScrollContentPresenter().SizeChanged += OnFileListViewScrollContentPresenterSizeChanged;
            ContentListView.GetScrollContentPresenter().SizeChanged += OnContentListViewScrollContentPresenterSizeChanged;
            AdjustHitListViewContentColumn();
            AdjustFileListViewPathColumn();
            AdjustContentListViewTextColumn();
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
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void OnHitListViewScrollContentPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustHitListViewContentColumn();
        }

        private void OnFileListViewScrollContentPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustFileListViewPathColumn();
        }

        private void OnContentListViewScrollContentPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustContentListViewTextColumn();
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
                    AdjustHitListViewContentColumn();
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

        private void OnHitListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HitListView.SelectedItem is HitItem h)
            {
                if (client.OnSelectionChanged(h))
                {
                    AdjustContentListViewTextColumn();
                }
                int v = h.Line - 1;
                ContentListView.SelectedIndex = v;
                ContentListView.ScrollIntoView(ContentListView.SelectedItem);
            }
        }

        private async void OnFileListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileListView.SelectedItem is FileItem f)
            {
                using (WorkInProgress.Create())
                {
                    if (await client.OnFileSelectionChanged(f))
                    {
                        AdjustContentListViewTextColumn();
                        if (ContentListView.ItemsSource is ObservableCollection<LineText> tt)
                        {
                            if (f.HitRows > 0 && HitListView.ItemsSource is ObservableCollection<HitItem> hh)
                            {
                                var h = hh.Where(x => x.Fid == f.Fid).FirstOrDefault();
                                if (h != null)
                                {
                                    ContentListView.SelectedItem = tt[h.Line - 1];
                                    ContentListView.ScrollIntoView(tt[h.Line - 1]);
                                }
                                else
                                {
                                    ContentListView.ScrollIntoView(tt[0]);
                                }
                            }
                            else
                            {
                                ContentListView.ScrollIntoView(tt[0]);
                            }
                        }
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
                        if (HitListView.SelectedItem is HitItem h && h.Fid != f.Fid)
                        {
                            HitListView.SelectedItem = hh.Where(x => x.Fid == f.Fid).FirstOrDefault();
                            if (HitListView.SelectedItem != null)
                            {
                                HitListView.ScrollIntoView(HitListView.SelectedItem);
                            }
                        }
                    }
                }
                UpperViewSwitchButton.Content = Properties.Resources.FileList;
            }
        }

        private void SwitchToFileList()
        {
            if (FileListView.Visibility == Visibility.Hidden)
            {
                HitListView.Visibility = Visibility.Hidden;
                FileListView.Visibility = Visibility.Visible;
                if (FileListView.ItemsSource is ObservableCollection<FileItem> ff)
                {
                    if (HitListView.SelectedItem is HitItem h)
                    {
                        FileListView.SelectedItem = ff.Where(x => x.Fid == h.Fid).FirstOrDefault();
                        if (FileListView.SelectedItem != null)
                        {
                            FileListView.ScrollIntoView(FileListView.SelectedItem);
                        }
                    }
                }
                UpperViewSwitchButton.Content = Properties.Resources.HitList;
            }
        }

        // Note:
        // ScrollContentPresenter's SizeChanged event is fired even when its ActualWidth is not changed.
        // As such, the adjustment for a column header needs to be done only when ActualWidth was changed.

        private void AdjustHitListViewContentColumn()
        {
            var wP = HitListView.GetScrollContentPresenter().ActualWidth;
            if (HitListViewScrollContentPresenterWidth != wP)
            {
                var w = wP - (HitListViewNameColumn.ActualWidth + HitListViewLineColumn.ActualWidth) - LISTVIEW_COLUMN_WIDTH_ADJUSTMENT;
                if (w > 0)
                {
                    HitListViewTextColumn.Width = w;
                }
                HitListViewScrollContentPresenterWidth = wP;
            }
        }

        private void AdjustFileListViewPathColumn()
        {
            var wP = FileListView.GetScrollContentPresenter().ActualWidth;
            if (FileListViewScrollContentPresenterWidth != wP)
            {
                var w = wP - FileListViewSizeColumn.ActualWidth - LISTVIEW_COLUMN_WIDTH_ADJUSTMENT;
                if (w > 0)
                {
                    FileListViewPathColumn.Width = w;
                }
                FileListViewScrollContentPresenterWidth = wP;
            }
        }

        private void AdjustContentListViewTextColumn()
        {
            var wP = ContentListView.GetScrollContentPresenter().ActualWidth;
            if (ContentListViewScrollContentPresenterWidth != wP)
            {
                var w = wP - ContentListViewLineColumn.ActualWidth - LISTVIEW_COLUMN_WIDTH_ADJUSTMENT;
                if (ContentListViewTextColumn.ActualWidth < w)
                {
                    ContentListViewTextColumn.Width = w;
                }
                ContentListViewScrollContentPresenterWidth = wP;
            }
        }
    }
}
