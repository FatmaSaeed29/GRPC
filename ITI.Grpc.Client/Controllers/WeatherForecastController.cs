using ITI.Grpc.Client;
using Microsoft.AspNetCore.Mvc;

namespace CityWeather.Grpc.Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Conditions = new[]
        {
            "Clear Skies", "Partly Cloudy", "Overcast", "Light Rain", "Heavy Rain", "Thunderstorms", "Snow", "Sleet", "Hail", "Fog"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        public object TemperatureC { get; private set; }

        [HttpGet(Name = "GetCityWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-10, 35),
                City = GetRandomCity() 
            })
            .ToArray();
        }

        private string GetRandomCity()
        {
            string[] cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix" };
            return cities[Random.Shared.Next(cities.Length)];
        }
    }
}
