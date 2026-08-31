using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Backend.Soap;
using Andrej_Kolega_IIS.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andrej_Kolega_IIS.Frontend.Controllers
{
    [Authorize]
    public class OrdersManageController : Controller
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FirebaseOrdersClient _firebaseClient;

        public OrdersManageController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            FirebaseOrdersClient firebaseClient)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _firebaseClient = firebaseClient;
        }

        private bool IsCustomMode =>
            string.Equals(_configuration["OrdersApi:Mode"], "Custom", StringComparison.OrdinalIgnoreCase);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new OrdersManageViewModel { Mode = IsCustomMode ? "Custom" : "Public" };

            if (TempData["StatusMessage"] is string statusMessage)
            {
                ViewData["StatusMessage"] = statusMessage;
            }

            if (IsCustomMode)
            {
                try
                {
                    var client = await CreateAuthorizedClientAsync();
                    var response = await client.GetAsync("api/custom/orders");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        model.CustomOrders = JsonSerializer.Deserialize<List<OrderDto>>(json, JsonOptions);
                    }
                    else
                    {
                        model.ErrorMessage = $"Custom API returned HTTP {(int)response.StatusCode}.";
                    }
                }
                catch (InvalidOperationException ex)
                {
                    model.ErrorMessage = ex.Message;
                }
            }
            else
            {
                model.PublicOrders = await _firebaseClient.FetchOrdersAsync();
            }

            return View(model);
        }

        [Authorize(Roles = "FullAccess")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new OrderFormViewModel());
        }

        [Authorize(Roles = "FullAccess")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderFormViewModel model)
        {
            var writeDto = ToWriteDto(model);
            if (writeDto.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "At least one item with a product name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = await CreateAuthorizedClientAsync();
                var response = await client.PostAsJsonAsync("api/custom/orders", writeDto);
                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = $"Failed to create order (HTTP {(int)response.StatusCode}).";
                    return View(model);
                }
            }
            catch (InvalidOperationException ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }

            TempData["StatusMessage"] = "Order created via the custom REST API.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "FullAccess")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await CreateAuthorizedClientAsync();
            var response = await client.GetAsync($"api/custom/orders/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var dto = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
            return View(FromDto(dto!));
        }

        [Authorize(Roles = "FullAccess")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderFormViewModel model)
        {
            var writeDto = ToWriteDto(model);
            if (writeDto.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "At least one item with a product name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = await CreateAuthorizedClientAsync();
                var response = await client.PutAsJsonAsync($"api/custom/orders/{id}", writeDto);
                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = $"Failed to update order (HTTP {(int)response.StatusCode}).";
                    return View(model);
                }
            }
            catch (InvalidOperationException ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }

            TempData["StatusMessage"] = "Order updated via the custom REST API.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "FullAccess")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteViaGraphQl(int id)
        {
            try
            {
                var token = await GetValidAccessTokenAsync();
                var client = _httpClientFactory.CreateClient("BackendApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var payload = new
                {
                    query = "mutation($id: Int!) { deleteOrder(id: $id) }",
                    variables = new { id }
                };

                var response = await client.PostAsJsonAsync("graphql", payload);
                var body = await response.Content.ReadAsStringAsync();

                TempData["StatusMessage"] = response.IsSuccessStatusCode && !body.Contains("\"errors\"")
                    ? "Order deleted via GraphQL."
                    : $"Failed to delete order via GraphQL: {body}";
            }
            catch (InvalidOperationException ex)
            {
                TempData["StatusMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<HttpClient> CreateAuthorizedClientAsync()
        {
            var token = await GetValidAccessTokenAsync();
            var client = _httpClientFactory.CreateClient("BackendApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task<string> GetValidAccessTokenAsync()
        {
            var accessToken = HttpContext.Session.GetString(SessionKeys.AccessToken);
            var expiresAtRaw = HttpContext.Session.GetString(SessionKeys.AccessTokenExpiresAtUtc);

            if (accessToken is not null && expiresAtRaw is not null &&
                DateTime.Parse(expiresAtRaw, null, DateTimeStyles.RoundtripKind) > DateTime.UtcNow.AddSeconds(30))
            {
                return accessToken;
            }

            var refreshToken = HttpContext.Session.GetString(SessionKeys.RefreshToken)
                ?? throw new InvalidOperationException("Your session has expired. Please log out and log back in.");

            var authClient = _httpClientFactory.CreateClient("BackendApi");
            var response = await authClient.PostAsJsonAsync("api/custom/auth/refresh", new { refreshToken });
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Your session has expired. Please log out and log back in.");
            }

            var tokens = await response.Content.ReadFromJsonAsync<TokenResponseDto>(JsonOptions)
                ?? throw new InvalidOperationException("Could not refresh the access token.");

            HttpContext.Session.SetString(SessionKeys.AccessToken, tokens.AccessToken);
            HttpContext.Session.SetString(SessionKeys.RefreshToken, tokens.RefreshToken);
            HttpContext.Session.SetString(SessionKeys.AccessTokenExpiresAtUtc, tokens.AccessTokenExpiresAtUtc.ToString("o"));

            return tokens.AccessToken;
        }

        private static OrderWriteDto ToWriteDto(OrderFormViewModel model)
        {
            return new OrderWriteDto
            {
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                OrderDate = model.OrderDate,
                Status = model.Status,
                ShippingCity = model.ShippingCity,
                Items = model.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.ProductName))
                    .Select(i => new OrderItemWriteDto
                    {
                        ProductName = i.ProductName!,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    })
                    .ToList()
            };
        }

        private static OrderFormViewModel FromDto(OrderDto dto)
        {
            var model = new OrderFormViewModel
            {
                Id = dto.Id,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                OrderDate = dto.OrderDate,
                Status = dto.Status,
                ShippingCity = dto.ShippingCity,
                Items = dto.Items.Select(i => new OrderItemFormRow
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            while (model.Items.Count < 3)
            {
                model.Items.Add(new OrderItemFormRow());
            }

            return model;
        }
    }
}
