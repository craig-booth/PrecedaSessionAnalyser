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
using System.Threading;
using System.Windows.Threading;


namespace PrecedaSessionAnalyser
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        public ImportWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (ImportViewModel)this.DataContext;
            viewModel.Password = txtPassword.Password;

           var importTask = viewModel.ImportData();
        }

    }

}
