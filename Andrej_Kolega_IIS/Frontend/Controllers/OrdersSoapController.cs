using System.ServiceModel;
using Andrej_Kolega_IIS.Backend.Soap;
using Andrej_Kolega_IIS.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andrej_Kolega_IIS.Frontend.Controllers
{
    [Authorize]
    public class OrdersSoapController : Controller
    {
        private readonly IConfiguration _configuration;

        public OrdersSoapController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View(new OrdersSoapSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(OrdersSoapSearchViewModel model)
        {
            model.Result = await CallSoapAsync(channel => channel.SearchOrdersByCustomerName(model.SearchTerm ?? string.Empty));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> Generate()
        {
            var result = await CallSoapAsync(channel => channel.GenerateOrdersXml());
            TempData["GenerateMessage"] = result.Success
                ? $"Generated orders.xml with {result.OrderCount} order(s) fetched from the REST API."
                : $"Failed to generate orders.xml: {result.ErrorMessage}";
            return RedirectToAction(nameof(Search));
        }

        private async Task<TResult> CallSoapAsync<TResult>(Func<IOrdersSoapService, Task<TResult>> call)
        {
            var baseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5183";
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress(new Uri(new Uri(baseUrl), "/soap/orders"));

            var factory = new ChannelFactory<IOrdersSoapService>(binding, endpoint);
            var channel = factory.CreateChannel();

            try
            {
                return await call(channel);
            }
            finally
            {
                ((ICommunicationObject)channel).Close();
                factory.Close();
            }
        }
    }
}
