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
using System.Windows.Shapes;

namespace com.hideakin.textsearch.view
{
    /// <summary>
    /// AuthWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AuthWindow : Window
    {
        public string Username => UsernameTextBox.Text;

        public string Password => PasswordTextBox.Password;

        public AuthWindow()
        {
            InitializeComponent();
            ApplyButton.IsEnabled = Username.Length > 0;
        }

        private void OnApply(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnUsernameChanged(object sender, TextChangedEventArgs e)
        {
            ApplyButton.IsEnabled = Username.Length > 0;
        }
    }
}
