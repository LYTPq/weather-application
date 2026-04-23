using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using WpfApp1.OpenWeatherAPI;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace WpfApp1
{

    /// <summary>
    /// Holds the OpenWeather API key loaded from <c>appsettings.json</c>.
    /// </summary>
    public class ApiSettings
    {
        public string OpenWeatherKey { get; set; }
    }


    /// <summary>
    /// Bootstraps the dependency-injection container, configures instances, and registers all application services
    /// We want to avoid duplicating HTTP logic while letting each service target a different API host
    /// </summary>
    public class httpFactory
    {

        public IServiceProvider InitServiceCollection()
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

            string apiKey = config["ApiKeys:OpenWeather"];

            ServiceCollection services = new ServiceCollection();

            services.AddSingleton(new ApiSettings { OpenWeatherKey = apiKey });

            // in order to avoid code multiplication by creating three classes with different interfaces, it is better to separate them just by interfaces since they implement one basic function, get some data from api
            services.AddHttpClient<IweatherAPI, ApiWork>(client =>
            {
                client.BaseAddress = new Uri("https://api.openweathermap.org");
            });

            services.AddHttpClient<IhourForecastAPI, ApiWork>(client =>
            {
                client.BaseAddress = new Uri("https://pro.openweathermap.org");
            });

            services.AddHttpClient<IdeterCityAPI, ApiWork>(client =>
            {
                client.BaseAddress = new Uri("http://ip-api.com/json");
            });


            services.AddSingleton<IdataLogic, DataLogic>();


            // service provider responsible for giving services
            IServiceProvider serviceProvider = services.BuildServiceProvider();


            return serviceProvider;
        }

    }

    /// <summary>
    /// Gives current weather, daily forecasts, coordinates, and air pollution data
    /// from the OpenWeather API.
    /// </summary>
    public interface IweatherAPI
    {

        Task<CurrentWeatherData> getCurrentWeather(string city);
        Task<DailyForecastArray> getDailyForecast(string city);
        Task<(double, double)> GetCoordinates(string city);
        Task<string> GetAirPollution(string city);


    }

    public interface IhourForecastAPI
    {
        Task<HourlyForecastArray> getHourlyForecast(string city);
    }


    /// <summary>
    /// Resolves the user's city based on IP geolocation.
    /// </summary>
    public interface IdeterCityAPI
    {
        Task<string> getCity();
    }

    /// <summary>
    /// Defines methods for processing and interpreting data types, such as timestamps, wind directions,
    /// visibility, and percentages
    /// </summary>
    public interface IdataLogic
    {

        /// <summary>
        /// Converts a Unix timestamp into a readable local date/time string
        /// </summary>
        /// <param name="timestamp">The Unix timestamp in seconds</param>
        /// <param name="number">Set to 0 for a full date/time string, anny other number returns only the day of the week</param>
        /// <returns> A formatted local time string </returns>
        string FromUnixTime(long timestamp, int number);

        // converts wind degrees to a 16-point compass label
        string WindDirection(long degree);

        // converts visibility in metres to (km string, category string)
        (string, string) Visibility(long visibility);

        // maps a humidity percentage to a category
        string PercentageCategory(int percentage);


        // Used for checking internet connection
        bool PingGoogle();

    }

    // When we ask for service from service provider it automatically passes the http client created and maintained by httpfactory and returns interface implementation
    // Thus we will have one different instance of the same class with method separation by interfaces and its own httpclient
    namespace OpenWeatherAPI
    {
        /// <summary>
        /// Implements all three API interfaces with one shared GetRequestAsync helper
        /// Each DI registration gives this class a different HttpClient base address
        /// </summary>
        public class ApiWork : IweatherAPI, IhourForecastAPI, IdeterCityAPI
        {

            private readonly HttpClient _httpClient;
            private readonly string _apiKey;

            public ApiWork(HttpClient httpClient, ApiSettings settings)
            {
                _httpClient = httpClient;
                _apiKey = settings.OpenWeatherKey;
            }

            public async Task<CurrentWeatherData> getCurrentWeather(string city)
            {
                string path = $"/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";    // cnt is the number of days
                return await GetRequestAsync<CurrentWeatherData>(path);
            }


            public async Task<HourlyForecastArray> getHourlyForecast(string city)
            {
                string path = $"/data/2.5/forecast/hourly?q={city}&appid={_apiKey}&units=metric";
                return await GetRequestAsync<HourlyForecastArray>(path);
            }

            public async Task<DailyForecastArray> getDailyForecast(string city)
            {
                string path = $"/data/2.5/forecast/daily?q={city}&cnt=8&appid={_apiKey}&units=metric";
                return await GetRequestAsync<DailyForecastArray>(path);
            }


            public async Task<string> getCity()
            {
                string path = "";
                DeterCity temp = await GetRequestAsync<DeterCity>(path);
                return temp.city;
            }



            /// <summary>
            /// Sends a GET request to the specified <paramref name="path"/>
            /// and deserializes the JSON response body into <typeparamref name="T"/>.
            /// </summary>
            /// <exception cref="HttpRequestException">Thrown if the response status is not successful.</exception>
            public async Task<T> GetRequestAsync<T>(string path)
            {
                HttpResponseMessage response = await _httpClient.GetAsync(path);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>();
            }

            // gets coordinates first, then queries the air pollution endpoint
            public async Task<string> GetAirPollution(string city)
            {
                string[] quality = ["Good", "Fair", "Moderate", "Poor", "Very Poor"];
                (double lat, double lon) = await GetCoordinates(city);
                string path = $"/data/2.5/air_pollution?lat={lat}&lon={lon}&appid={_apiKey}";
                AirPollution pollution = await GetRequestAsync<AirPollution>(path);
                return quality[pollution.list[0].main.aqi - 1];
            }


            
            public async Task<(double, double)> GetCoordinates(string city)
            {
                string path = $"/geo/1.0/direct?q={city}&limit=5&appid={_apiKey}";
                Geolocation[] data = await GetRequestAsync<Geolocation[]>(path);
                double lon = data[0].lon;
                double lat = data[0].lat;
                return (lat, lon);
            }

        }

    }

    /// <summary>
    /// Responsible for formatting, categorization, and connectivity-check helpers
    /// </summary>
    public class DataLogic : IdataLogic
    {

        public string FromUnixTime(long timestamp, int number)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            if (number == 0)
            {
                return dto.LocalDateTime.ToString();
            }
            else
            {
                return dto.LocalDateTime.ToString("dddd", new CultureInfo("en-US"));
            }
        }

        public string WindDirection(long degree)
        {
            // divide 360 degree into 16 compass sectors of 22.5 degree each
            int temp = (int)((degree / 22.5) + .5);
            string[] arr = [ "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" ];
            return arr[temp % 16];
        }


        
        public (string, string) Visibility(long visibility)
        {

            double km = visibility / 1000;

            string category = visibility switch
            {
                >= 10000 => "Excellent",
                >= 4000 => "Good",
                >= 2000 => "Moderate",
                >= 800 => "Poor",
                _ => "Very poor"
            };

            return (km.ToString(), category);

        }

        public string PercentageCategory(int percentage)
        {
            string category = percentage switch
            {
                >= 81 => "Excellent",
                >= 61 => "Good",
                >= 41 => "Moderate",
                >= 21 => "Poor",
                _ => "Very poor"

            };

            return category;

        }

        public bool PingGoogle()
        {
            try
            {
                Ping ping = new Ping();
                var reply = ping.Send("google.com", 1000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }


    }

    public static class ApiExceptionHandler
    {
        public static void Handle(Exception ex)
        {
            string message = ex switch
            {
                HttpRequestException e => e.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "City not found",
                    System.Net.HttpStatusCode.Unauthorized => "Invalid API key",
                    System.Net.HttpStatusCode.TooManyRequests => "API rate limit exceeded",
                    System.Net.HttpStatusCode.ServiceUnavailable => "Weather service is down",
                    _ => $"Unexpected error: {e.StatusCode}"
                },
                TaskCanceledException => "Request timed out",
                System.Text.Json.JsonException => "Received unexpected data from the server",
                _ => "Something went wrong"
            };
            MessageBox.Show(message);
        }
    }

}
