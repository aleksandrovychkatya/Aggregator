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

namespace Aggregator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<Category> categories = DiaexpertParser.makeAllCategories();
            Product firstProduct = categories.First().Products.First();

            LabelName.Content = firstProduct.Name;
            LabelPrice.Content = firstProduct.Price;
            LBShortDescr.Text = firstProduct.ShortDescription;
            LBFullDescr.Items.Add(firstProduct.FullDescription);

            var fullFilePath = firstProduct.Image;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();

            ProductImage.Source = bitmap;
        }
    }
}
