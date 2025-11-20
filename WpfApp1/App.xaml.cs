using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            
            var factory = new httpFactory();
            var services = factory.InitServiceCollection();
            var weatherApi = services.GetRequiredService<IweatherAPI>();
            var hourApi = services.GetRequiredService<IhourForecastAPI>();
            var cityApi = services.GetRequiredService<IdeterCityAPI>();
            var appLogic = services.GetRequiredService<IdataLogic>();

            if (!appLogic.PingGoogle())
            {
                MessageBox.Show("Problems with internet connection");
                return;
            }

            
            var vm = new MainViewModel(weatherApi, hourApi, cityApi, appLogic);


            vm.LoadAsync();

            // now viewmodel is the property of datacontext
            // datacontext responsible for binding the data in windows 

            var wnd = new MainWindow();
            wnd.DataContext = vm;

            wnd.Show();
        }
    }

}
