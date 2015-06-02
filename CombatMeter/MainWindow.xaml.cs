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
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace CombatMeter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        CombatListViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new CombatListViewModel();

            this.DataContext = viewModel.DataContext;

        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ParseFile();
            //todo: window for selecting file(s?) and parse filename+path
        }

        private void LiverParserButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.StartLiveParser();

        }

    }
}
