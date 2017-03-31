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
using System.Globalization;
using System.IO;

using LiveCharts;
using LiveCharts.Wpf;

namespace PrecedaSessionAnalyser
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dataBasePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrecedaSessions.db");
            var importViewModel = new ImportViewModel(dataBasePath);

            var importWindow = new ImportWindow();
            importWindow.DataContext = importViewModel;

            importWindow.Show();
        }
    }

}
