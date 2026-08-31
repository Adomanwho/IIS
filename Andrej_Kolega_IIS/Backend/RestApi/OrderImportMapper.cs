using Andrej_Kolega_IIS.Backend.RestApi.Dto;
using Andrej_Kolega_IIS.Shared.Entities;

namespace Andrej_Kolega_IIS.Backend.RestApi
{
    public static class OrderImportMapper
    {
        public static OrdersImportDto ToImportDto(OrdersXmlDto xmlDto)
        {
            return new OrdersImportDto
            {
                Orders = xmlDto.Orders.Select(o => new OrderImportDto
                {
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    OrderDate = DateTime.Parse(o.OrderDate),
                    Status = o.Status,
                    ShippingCity = o.ShippingCity,
                    Items = o.Items.Select(i => new OrderItemImportDto
                    {
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                }).ToList()
            };
        }

        public static OrdersXmlDto ToXmlDto(List<OrderImportDto> orders)
        {
            return new OrdersXmlDto
            {
                Orders = orders.Select(o => new OrderXmlDto
                {
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    OrderDate = o.OrderDate.ToString("yyyy-MM-dd"),
                    Status = o.Status,
                    ShippingCity = o.ShippingCity,
                    Items = o.Items.Select(i => new OrderItemXmlDto
                    {
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                }).ToList()
            };
        }

        public static List<Order> ToEntities(OrdersImportDto dto)
        {
            return dto.Orders.Select(o => new Order
            {
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail,
                OrderDate = o.OrderDate,
                Status = Enum.Parse<OrderStatus>(o.Status),
                ShippingCity = o.ShippingCity,
                Items = o.Items.Select(i => new OrderItem
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }).ToList();
        }
    }
}
