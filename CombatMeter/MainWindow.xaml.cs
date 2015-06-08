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
        bool LiveParserRunning;



        public MainWindow()
        {
            InitializeComponent();
            viewModel = new CombatListViewModel();

            this.DataContext = viewModel.DataContext;

            CombatLogListView.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;


        }


        // to scroll down, look at!
        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (CombatLogListView.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                var item = CombatLogListView.Items[CombatLogListView.Items.Count - 1];

                if (item == null)
                {
                    return;
                }
                    CombatLogListView.ScrollIntoView(item);
            }
        }
        

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            openFileDlg.DefaultExt = ".txt";
            openFileDlg.Filter = "Text documents (.txt)|*.txt";
            openFileDlg.Multiselect = true;
            openFileDlg.InitialDirectory = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\Star Wars - The Old Republic\CombatLogs\");

            var itemsWereSelected = openFileDlg.ShowDialog();

            if (itemsWereSelected == true)
            {
                viewModel.ParseFiles(openFileDlg.FileNames);
            }            
        }

        private void LiverParserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LiveParserRunning)
            {
                
                LiverParserButton.Background = Brushes.LightGreen;
                OpenFileButton.IsEnabled = false; //disable to prevent opening during liveparsing.
                viewModel.StartLiveParser();
                LiveParserRunning = true;
            }
            else
            {
                
                LiverParserButton.Background = SystemColors.WindowBrush;
                OpenFileButton.IsEnabled = true;
                viewModel.StopLiveParser();
                LiveParserRunning = false;
            }                        
        }

    }
}
