using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Andrej_Kolega_IIS.Backend.CustomApi.GraphQL
{
    public class OrdersQuery
    {
        public async Task<List<OrderDto>> GetOrders([Service] AppDbContext context)
        {
            var orders = await context.Orders.Include(o => o.Items).ToListAsync();
            return orders.Select(OrderDtoMapper.ToDto).ToList();
        }

        public async Task<OrderDto?> GetOrder(int id, [Service] AppDbContext context)
        {
            var order = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            return order is null ? null : OrderDtoMapper.ToDto(order);
        }
    }
}
