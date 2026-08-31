using Andrej_Kolega_IIS.Backend.Grpc;
using Andrej_Kolega_IIS.Frontend.Models;
using global::Grpc.Core;
using global::Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andrej_Kolega_IIS.Frontend.Controllers
{
    [Authorize]
    public class WeatherController : Controller
    {
        private readonly IConfiguration _configuration;

        public WeatherController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View(new WeatherSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(WeatherSearchViewModel model)
        {
            var baseUrl = _configuration["Grpc:BaseUrl"] ?? "http://localhost:5184";
            using var channel = GrpcChannel.ForAddress(baseUrl);
            var client = new WeatherService.WeatherServiceClient(channel);

            try
            {
                var response = await client.GetTemperatureByCityAsync(
                    new CityTemperatureRequest { CityQuery = model.CityQuery ?? string.Empty });

                model.Results = response.Results.ToList();
            }
            catch (RpcException ex)
            {
                model.ErrorMessage = $"gRPC call failed: {ex.Status.Detail}";
            }

            return View(model);
        }
    }
}
