using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // now we get the same object as we set before in prop of datacontext
        private MainViewModel vm => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }


        public async void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string request = search.Text;
                if (string.IsNullOrWhiteSpace(request)) return;

                try { await vm.RefreshAsync(request); }
                catch (Exception ex) { ApiExceptionHandler.Handle(ex); }
            }
        }

        private void DailyForecast_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is DailyForecastData day)
            {
                vm.SelectedDailyForecast = day;
            }
        }
    }
}