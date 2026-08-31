using System.Text.Json;
using Andrej_Kolega_IIS.Backend.RestApi.Dto;
using Andrej_Kolega_IIS.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andrej_Kolega_IIS.Frontend.Controllers
{
    [Authorize(Roles = "FullAccess")]
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new OrderImportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(OrderImportViewModel model)
        {
            if (model.File is null || model.File.Length == 0)
            {
                model.ErrorMessage = "Please choose a file to upload.";
                return View(model);
            }

            var extension = System.IO.Path.GetExtension(model.File.FileName).ToLowerInvariant();
            var endpoint = extension switch
            {
                ".xml" => "api/rest/orders/xml",
                ".json" => "api/rest/orders/json",
                _ => null
            };

            if (endpoint is null)
            {
                model.ErrorMessage = "Only .xml or .json files are supported.";
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("BackendApi");

            using var content = new MultipartFormDataContent();
            await using var stream = model.File.OpenReadStream();
            using var streamContent = new StreamContent(stream);
            content.Add(streamContent, "file", model.File.FileName);

            var response = await client.PostAsync(endpoint, content);
            var body = await response.Content.ReadAsStringAsync();

            model.Result = JsonSerializer.Deserialize<ImportResult>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(model);
        }
    }
}
