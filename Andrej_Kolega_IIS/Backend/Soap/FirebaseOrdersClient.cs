using System.Text.Json;
using Andrej_Kolega_IIS.Backend.RestApi.Dto;

namespace Andrej_Kolega_IIS.Backend.Soap
{
    public class FirebaseOrdersClient
    {
        private readonly HttpClient _httpClient;

        public FirebaseOrdersClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<OrderImportDto>> FetchOrdersAsync()
        {
            var response = await _httpClient.GetAsync("orders.json");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body == "null")
            {
                return new List<OrderImportDto>();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var orders = JsonSerializer.Deserialize<Dictionary<string, OrderImportDto>>(body, options);

            return orders?.Values.ToList() ?? new List<OrderImportDto>();
        }
    }
}
