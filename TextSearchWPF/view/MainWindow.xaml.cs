using System;
using System.Collections.Generic;
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

        private const double LISTVIEW_COLUMN_WIDTH_PLAY = 30.0;
        private bool needToResizeHitListViewColumns = true;
        private bool needToResizeContentListViewColumns = true;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = client;
            StatusBarLabel.Content = " ";
            Dispatcher.BeginInvoke(new EventHandler<RoutedEventArgs>(OnLoaded), this, new RoutedEventArgs());
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            using (new TemporalText(StatusBarLabel, Properties.Resources.Initializing))
            {
                await client.Initialize();
            }
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
                using (new WaitCursor())
                {
                    authenticated = await client.Authenticate(dialogBox.Username, dialogBox.Password);
                }
                if (authenticated)
                {
                    MessageBox.Show(Properties.Resources.AuthenticateSuccess, Properties.Resources.AuthWindowTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    using (new WaitCursor())
                    using (new TemporalText(StatusBarLabel, Properties.Resources.WaitingForResponse))
                    {
                        await client.UpdateGroups();
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.AuthenticateFailure, Properties.Resources.AuthWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void OnReloadGroups(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            using (new DisableControl(EditReloadGroupsMenuItem))
            using (new TemporalText(StatusBarLabel, Properties.Resources.WaitingForResponse))
            {
                var last = GroupComboBox.SelectedItem;
                await client.UpdateGroups();
                if (last != null && GroupComboBox.HasItems && GroupComboBox.Items.Contains(last))
                {
                    GroupComboBox.SelectedItem = last;
                }
            }
        }

        private void OnViewClear(object sender, RoutedEventArgs e)
        {
            client.Clear();
            StatusBarLabel.Content = " ";
            needToResizeHitListViewColumns = true;
            needToResizeContentListViewColumns = true;
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

        private async void OnQueryStart(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            using (new DisableControl(QueryButton))
            using (var tt = new TemporalText(StatusBarLabel, Properties.Resources.WaitingForResponse))
            {
                var message = await client.Execute();
                if (message == null)
                {
                    if (needToResizeHitListViewColumns)
                    {
                        ResizeHitListViewColumns();
                    }
                    tt.Reset(Properties.Resources.HitFormat, client.HitItems.Count);
                }
                else
                {
                    MessageBox.Show(message, Properties.Resources.AppCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
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
                    if (needToResizeContentListViewColumns)
                    {
                        ResizeContentListViewColumns();
                    }
                }
                int v = h.Line - 1;
                ContentListView.SelectedIndex = v;
                ContentListView.ScrollIntoView(ContentListView.SelectedItem);
            }
        }

        private void ResizeHitListViewColumns()
        {
            var w = HitListView.ActualWidth - (HitListViewNameColumn.ActualWidth + HitListViewLineColumn.ActualWidth) - LISTVIEW_COLUMN_WIDTH_PLAY;
            if (w > 0)
            {
                HitListViewTextColumn.Width = w;
                needToResizeHitListViewColumns = false;
            }
        }

        private void ResizeContentListViewColumns()
        {
            var w = ContentListView.ActualWidth - ContentListViewLineColumn.ActualWidth - LISTVIEW_COLUMN_WIDTH_PLAY;
            if (w > 0)
            {
                ContentListViewTextColumn.Width = w;
                needToResizeContentListViewColumns = false;
            }
        }
    }
}
