using System.Xml.XPath;
using Andrej_Kolega_IIS.Backend.RestApi.Validation;
using Andrej_Kolega_IIS.Backend.Soap.Dto;

namespace Andrej_Kolega_IIS.Backend.Soap
{
    public class OrdersSoapService : IOrdersSoapService
    {
        private readonly OrdersXmlGenerator _xmlGenerator;
        private readonly OrderXmlValidator _xmlValidator;

        public OrdersSoapService(OrdersXmlGenerator xmlGenerator, OrderXmlValidator xmlValidator)
        {
            _xmlGenerator = xmlGenerator;
            _xmlValidator = xmlValidator;
        }

        public async Task<GenerateXmlResult> GenerateOrdersXml()
        {
            try
            {
                var count = await _xmlGenerator.GenerateAsync();
                return new GenerateXmlResult { Success = true, OrderCount = count };
            }
            catch (Exception ex)
            {
                return new GenerateXmlResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<SearchOrdersResult> SearchOrdersByCustomerName(string customerNameTerm)
        {
            if (!File.Exists(_xmlGenerator.XmlPath))
            {
                await _xmlGenerator.GenerateAsync();
            }

            using (var validateStream = File.OpenRead(_xmlGenerator.XmlPath))
            {
                var errors = _xmlValidator.Validate(validateStream);
                if (errors.Count > 0)
                {
                    return new SearchOrdersResult { ValidationPassed = false, ValidationErrors = errors };
                }
            }

            var term = customerNameTerm ?? string.Empty;
            var expression = $"//Order[contains({XPathHelper.ToLowerCaseExpression("CustomerName")}, " +
                              $"{XPathHelper.ToStringLiteral(term.ToLowerInvariant())})]";

            var document = new XPathDocument(_xmlGenerator.XmlPath);
            var navigator = document.CreateNavigator();
            var nodes = navigator.Select(expression);

            var results = new List<OrderSearchItem>();
            while (nodes.MoveNext())
            {
                var order = nodes.Current!;
                var item = new OrderSearchItem
                {
                    CustomerName = order.SelectSingleNode("CustomerName")?.Value ?? string.Empty,
                    CustomerEmail = order.SelectSingleNode("CustomerEmail")?.Value ?? string.Empty,
                    OrderDate = order.SelectSingleNode("OrderDate")?.Value ?? string.Empty,
                    Status = order.SelectSingleNode("Status")?.Value ?? string.Empty,
                    ShippingCity = order.SelectSingleNode("ShippingCity")?.Value ?? string.Empty
                };

                var itemNodes = order.Select("Items/Item");
                while (itemNodes.MoveNext())
                {
                    var line = itemNodes.Current!;
                    item.Items.Add(new OrderSearchItemLine
                    {
                        ProductName = line.SelectSingleNode("ProductName")?.Value ?? string.Empty,
                        Quantity = int.TryParse(line.SelectSingleNode("Quantity")?.Value, out var qty) ? qty : 0,
                        UnitPrice = decimal.TryParse(line.SelectSingleNode("UnitPrice")?.Value, out var price) ? price : 0m
                    });
                }

                results.Add(item);
            }

            return new SearchOrdersResult { ValidationPassed = true, Orders = results };
        }
    }
}
