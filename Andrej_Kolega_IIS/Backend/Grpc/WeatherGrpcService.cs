using global::Grpc.Core;

namespace Andrej_Kolega_IIS.Backend.Grpc
{
    public class WeatherGrpcService : WeatherService.WeatherServiceBase
    {
        private readonly DhmzWeatherClient _weatherClient;

        public WeatherGrpcService(DhmzWeatherClient weatherClient)
        {
            _weatherClient = weatherClient;
        }

        public override async Task<CityTemperatureResponse> GetTemperatureByCity(CityTemperatureRequest request, ServerCallContext context)
        {
            var query = (request.CityQuery ?? string.Empty).Trim();

            List<CityWeatherData> allCities;
            try
            {
                allCities = await _weatherClient.FetchAllAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
            }

            var matches = allCities.Where(c => c.CityName.Contains(query, StringComparison.OrdinalIgnoreCase));

            var response = new CityTemperatureResponse();
            response.Results.AddRange(matches.Select(m => new CityTemperature
            {
                CityName = m.CityName,
                TemperatureCelsius = m.TemperatureCelsius
            }));

            return response;
        }
    }
}
