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
    public partial class UrlWindow : Window
    {
        public string Url
        {
            get
            {
                if (UrlComboBox.Text is string t)
                {
                    return t;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public UrlWindow()
        {
            InitializeComponent();
        }

        public void Add(string value, bool selected = false)
        {
            UrlComboBox.Items.Add(value);
            if (selected)
            {
                UrlComboBox.SelectedItem = value;
            }
        }

        private void OnApply(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
