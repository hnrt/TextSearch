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

namespace com.hideakin.textsearch.view
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextSearchClient client = new TextSearchClient();

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
            {
                await client.Initialize();
            }
        }

        private void fileExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnFileAuthMenuItemClick(object sender, RoutedEventArgs e)
        {
            var dialogBox = new AuthWindow();
            dialogBox.Owner = this;
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
                }
                else
                {
                    MessageBox.Show(Properties.Resources.AuthenticateFailure, Properties.Resources.AuthWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void OnQueryStart(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            using (new DisableControl(QueryButton))
            {
                StatusBarLabel.Content = "Waiting for response...";
                var message = await client.Execute();
                if (message == null)
                {
                    StatusBarLabel.Content = string.Format("Hit: {0}", client.HitItems.Count);
                }
                else
                {
                    MessageBox.Show(message, "TextSearch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusBarLabel.Content = " ";
                }
            }
        }

        private void OnHitListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HitListView.SelectedItem is HitItem h)
            {
                client.OnSelectionChanged(h);
                int v = h.Line - 1;
                ContentListView.SelectedIndex = v;
                ContentListView.ScrollIntoView(ContentListView.SelectedItem);
            }
        }
    }
}
