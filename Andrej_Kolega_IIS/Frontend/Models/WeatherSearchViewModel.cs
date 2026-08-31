using Andrej_Kolega_IIS.Backend.Grpc;

namespace Andrej_Kolega_IIS.Frontend.Models
{
    public class WeatherSearchViewModel
    {
        public string? CityQuery { get; set; }
        public List<CityTemperature>? Results { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
