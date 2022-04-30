using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace OnePass.WPF.Windows
{
    public class ProductDetails
    {
        public string Name { get; set; }

        public string Username { get; set; } = "Username";

        public string EmailAddress { get; set; } = "Email@email.com";

        public string Password { get; set; }

        public bool Favourite { get; set; }

        public bool MfaEnabled { get; set; }

        public string Notes { get; set; }

        public DateTime? LastChanged { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Interaction logic for ContentWindow.xaml
    /// </summary>
    public partial class ContentWindow : Window
    {
        public ObservableCollection<ProductDetails> ProductDetails { get; set; }

        public ContentWindow()
        {
            InitializeComponent();
            DataContext = this;

            ProductDetails = new ObservableCollection<ProductDetails>()
            {
                new ProductDetails() { Name = "Natwest" },
                new ProductDetails() { Name = "Microsoft" },
                new ProductDetails() { Name = "GMail" },
            };
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as ListView;
            var selected = item.SelectedItem as ProductDetails;

            ProductDetailsGrid.Visibility = Visibility.Visible;
            ProductDetailsGrid.DataContext = selected;
        }
    }
}
