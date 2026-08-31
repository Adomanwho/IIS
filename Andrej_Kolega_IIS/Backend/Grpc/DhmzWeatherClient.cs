using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Andrej_Kolega_IIS.Backend.Grpc
{
    public record CityWeatherData(string CityName, double TemperatureCelsius);

    public class DhmzWeatherClient
    {
        private const string CacheKey = "dhmz-weather-data";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public DhmzWeatherClient(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<List<CityWeatherData>> FetchAllAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<CityWeatherData>? cached) && cached is not null)
            {
                return cached;
            }

            var results = await FetchAndParseWithRetryAsync();
            _cache.Set(CacheKey, results, CacheDuration);
            return results;
        }

        private async Task<List<CityWeatherData>> FetchAndParseWithRetryAsync()
        {
            const int maxAttempts = 2;
            Exception? lastError = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var xml = await _httpClient.GetStringAsync("hrvatska_n.xml");
                    return ParseCities(XDocument.Parse(xml));
                }
                catch (Exception ex) when (ex is HttpRequestException or System.Xml.XmlException)
                {
                    lastError = ex;
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(500);
                    }
                }
            }

            throw new InvalidOperationException("Unable to fetch or parse the DHMZ weather feed.", lastError);
        }

        private static List<CityWeatherData> ParseCities(XDocument document)
        {
            var results = new List<CityWeatherData>();
            foreach (var gradElement in document.Root?.Elements("Grad") ?? Enumerable.Empty<XElement>())
            {
                var name = gradElement.Element("GradIme")?.Value.Trim();
                var tempText = gradElement.Element("Podatci")?.Element("Temp")?.Value.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (double.TryParse(tempText, NumberStyles.Float, CultureInfo.InvariantCulture, out var temperature))
                {
                    results.Add(new CityWeatherData(name, temperature));
                }
            }

            return results;
        }
    }
}
