using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Shared.Entities;

namespace Andrej_Kolega_IIS.Backend.CustomApi
{
    public static class OrderDtoMapper
    {
        public static OrderDto ToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                ShippingCity = order.ShippingCity,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }

        public static void ApplyTo(Order order, OrderWriteDto dto, OrderStatus status)
        {
            order.CustomerName = dto.CustomerName;
            order.CustomerEmail = dto.CustomerEmail;
            order.OrderDate = dto.OrderDate;
            order.Status = status;
            order.ShippingCity = dto.ShippingCity;
            order.Items = dto.Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();
        }
    }
}
