using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace WpfApp1
{

    /// <summary>
    /// Primary view model for the weather dashboard.
    /// Coordinates API calls and regulates weather data for XAML binding.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {

        private readonly IweatherAPI _weatherApi;
        private readonly IhourForecastAPI _hourApi;
        private readonly IdeterCityAPI _cityApi;
        private readonly IdataLogic _appLogic;
        private string _currentCity;

        private CurrentWeatherData _currentWeather;


        public CurrentWeatherData CurrentWeather
        {
            get => _currentWeather;
            private set { _currentWeather = value; OnPropertyChanged(); }
        }

        private string _currentDateTime;
        public string CurrentDateTime
        {
            get => _currentDateTime;
            set { _currentDateTime = value; OnPropertyChanged(); }
        }

        private string _currentRain;
        public string CurrentRain
        {
            get => _currentRain;
            set { _currentRain = value; OnPropertyChanged(); }
        }

        private string _windDirection;

        public string WindDirection
        {
            get => _windDirection;
            set { _windDirection = value; OnPropertyChanged(); }
        }


        private string _sunset;

        public string Sunset
        {
            get => _sunset;
            set { _sunset = value; OnPropertyChanged(); }
        }


        private string _sunrise ;

        public string Sunrise
        {
            get => _sunrise;
            set { _sunrise = value; OnPropertyChanged(); }
        }

        private string _pollution;
        public string Pollution
        {
            get => _pollution;
            set { _pollution = value; OnPropertyChanged(); }
        }

        private string _humidityCategory;
        public string HumidityCategory
        {
            get => _humidityCategory;
            set { _humidityCategory = value; OnPropertyChanged(); }
        }


        private string _visibility;
        public string Visibility
        {
            get => _visibility;
            set { _visibility = value; OnPropertyChanged(); }
        }

        private string _visibilityCategory;
        public string VisibilityCategory
        {
            get => _visibilityCategory;
            set { _visibilityCategory = value; OnPropertyChanged(); }
        }


        private string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            set { _iconPath = value; OnPropertyChanged(); }
        }

        public string _weekDay;
        public string WeekDay
        {
            get => _weekDay;
            set { _weekDay = value; OnPropertyChanged(); }
        }


        // automatically notifies wpf when items are added or removed.

        private ObservableCollection<DailyForecastData> _dailyForecast;
        public ObservableCollection<DailyForecastData> DailyForecast
        {
            get => _dailyForecast;
            set { _dailyForecast = value; OnPropertyChanged(); }
        }

        private ObservableCollection<HourlyForecastData> _hourlyForecast;
        public ObservableCollection<HourlyForecastData> HourlyForecast 
        {
            get => _hourlyForecast;
            set { _hourlyForecast = value; OnPropertyChanged(); }
        }


        private DailyForecastData _selectedDailyForecast;
        public DailyForecastData SelectedDailyForecast
        {
            get => _selectedDailyForecast;
            set
            {
                _selectedDailyForecast = value;
                OnPropertyChanged();
                if (value != null)
                {
                    ApplyDailyForecast(value);
                }
            }
        }


        public MainViewModel(
            IweatherAPI weatherApi,
            IhourForecastAPI hourApi,
            IdeterCityAPI cityApi,
            IdataLogic appLogic)
        {
            _weatherApi = weatherApi;
            _hourApi = hourApi;
            _cityApi = cityApi;
            _appLogic = appLogic;
             
        }


        // pushes weather data into the formatted display properties
        // (date/time, wind direction, sunrise/sunset, humidity category, icon)

        private void UpdateDisplayProperties(CurrentWeatherData data)
        {
            CurrentWeather = data;
            CurrentDateTime = $"{_appLogic.FromUnixTime(data.unixDate, 0).Remove(0, 11)}, {_appLogic.FromUnixTime(data.unixDate, 1)}";
            WindDirection = _appLogic.WindDirection(data.wind.deg);
            Sunrise = _appLogic.FromUnixTime(data.sys.sunrise, 0).Remove(0, 11);
            Sunset = _appLogic.FromUnixTime(data.sys.sunset, 0).Remove(0, 11);
            HumidityCategory = _appLogic.PercentageCategory(data.main.humidity);
            IconPath = $"Images/{data.weather[0].icon}.png";
        }


        // First time initialization.
        public async Task LoadAsync()
        {
            string city = await _cityApi.getCity();
            await RefreshAsync(city);
        }

        /// <summary>
        /// Fetches all weather data for the given city and updates every bound property
        /// Called on startup and whenever the user searches for a new city
        /// </summary>
        public async Task RefreshAsync(string city)
        {
            _currentCity = city;


            CurrentWeather = await _weatherApi.getCurrentWeather(city);
            Pollution = await _weatherApi.GetAirPollution(city);
            (Visibility, VisibilityCategory) = _appLogic.Visibility(CurrentWeather.visibility);


            UpdateDisplayProperties(CurrentWeather);

            DailyForecastArray dailyForecast = await _weatherApi.getDailyForecast(city);

            HourlyForecastArray hourlyForecast = await _hourApi.getHourlyForecast(city);


            // image parsing based on the info from the api 
            for (int i = 0; i < 7; i++)
            {
                // truncate day name to 3 characters 
                dailyForecast.list[i].DayOfWeek = _appLogic.FromUnixTime(dailyForecast.list[i].unixDate,1).Remove(3);

                dailyForecast.list[i].iconPath = $"Images/{dailyForecast.list[i].weather[0].icon}.png";
            }

            DailyForecast = new ObservableCollection<DailyForecastData>((dailyForecast.list.Take(7)));

            var src = hourlyForecast.list;
            var result = new ObservableCollection<HourlyForecastData>();

            // pick every 3rd entry to get 3-hour intervals, max 7 items
            for (int i = 0; i < src.Length && result.Count < 7; i += 3)
            {
                src[i].time = _appLogic.FromUnixTime(src[i].dt, 0).Substring(11,5);
                result.Add(src[i]);
            }

            HourlyForecast = result;

            // If there is no rain data in the call, set it to zero manually 
            if (CurrentWeather.rain != null && CurrentWeather.rain.onehour > 0)
                CurrentRain = $"{CurrentWeather.rain.onehour}";
            else
                CurrentRain = "0";


        }

        // maps a daily forecast entry into a CurrentWeatherData so the main
        // panel can display that day using the same bindings as today's weather
        private void ApplyDailyForecast(DailyForecastData day)
        {

            CurrentWeatherData temp = new CurrentWeatherData
            {
                main = new Main
                {
                    temp = day.temp.day,
                    temp_min = day.temp.min,
                    temp_max = day.temp.max,
                    humidity = day.humidity
                },
                wind = new Wind
                {
                    speed = day.speed,
                    deg = day.deg
                },
                sys = new Sys
                {
                    sunrise = day.sunrise,
                    sunset = day.sunset
                },
                weather = day.weather,
                unixDate = day.unixDate,
                name = _currentCity,
               
            };

            UpdateDisplayProperties(temp);
            CurrentRain = day.rain.ToString();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

}
