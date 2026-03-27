using System;
using System.Text.Json.Serialization;

namespace WpfApp1
{
    public class Coordinates
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Weather
    {
        public string description { get; set; }
        public string icon { get; set; }
    }
    public class Main
    {
        public double temp { get; set; }
        public double feels_like { get; set; }

        public double temp_min { get; set; }

        public double temp_max { get; set; }
        public int humidity { get; set; }
        public int pressure { get; set; }

    }

    public class Wind
    {
        public double speed { get; set; }
        public int deg { get; set; }
    }

    public class Sys
    {
        public long sunrise { get; set; }
        public long sunset { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }


    public class Rain
    {
        [JsonPropertyName("1h")]
        public double onehour { get; set; }
    }

    // current weather 
    public class CurrentWeatherData
    {
        public Coordinates coord { get; set; }
        public Weather[] weather { get; set; }
        public Main main { get; set; }

        public long visibility { get; set; }
        public Wind wind { get; set; }
        public Sys sys { get; set; }
        public Clouds clouds { get; set; }

        public Rain rain { get; set; }

        [JsonPropertyName("dt")]
        public long unixDate { get; set; }

        public long timezone { get; set; }
        public string? name { get; set; }
    }

    public class Temperature
    {
        public double day { get; set; }
        public double night { get; set; }

        public double min { get; set; }
        public double max { get; set; }
    }

    //structure of day forecast 
    public class DailyForecastData
    {

        [JsonPropertyName("dt")]
        public long unixDate { get; set; }

        public long sunrise { get; set; }
        public long sunset { get; set; }

        public Temperature? temp { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }

        public long visibility { get; set; }
        public Weather[]? weather { get; set; }
        public double speed { get; set; }
        public int deg { get; set; }

        public double rain { get; set; }

        public string DayOfWeek { get; set; }

        public string iconPath { get; set; }

    }

    // structure of hourly forecast 
    public class HourlyForecastData
    {
        public Main? main { get; set; }
        public Clouds? clouds { get; set; }

        public Wind? wind { get; set; }

        public long dt { get; set; }

        public string time { get; set; }

    }


    public class DailyForecastArray
    {
        public DailyForecastData[]? list { get; set; }
    }

    public class HourlyForecastArray
    {
        public HourlyForecastData[]? list { get; set; }
    }


    public class DeterCity
    {
        public string? city { get; set; }
    }

    public class Geolocation
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }



    public class MainAQI
    {
        public int aqi { get; set; }
    }

    public class AirPollutionRoot
    {
        public MainAQI main { get; set; }

    }

    public class AirPollution
    {
        public AirPollutionRoot[] list { get; set; }
    }
}
