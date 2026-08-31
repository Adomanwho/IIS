using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Shared.Data;
using Andrej_Kolega_IIS.Shared.Entities;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;

namespace Andrej_Kolega_IIS.Backend.CustomApi.GraphQL
{
    public class OrdersMutation
    {
        public async Task<OrderDto> CreateOrder(
            OrderWriteDto input,
            [Service] AppDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            RequireFullAccess(httpContextAccessor);
            var status = ParseStatus(input.Status);

            var order = new Order();
            OrderDtoMapper.ApplyTo(order, input, status);

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            return OrderDtoMapper.ToDto(order);
        }

        public async Task<OrderDto> UpdateOrder(
            int id,
            OrderWriteDto input,
            [Service] AppDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            RequireFullAccess(httpContextAccessor);
            var status = ParseStatus(input.Status);

            var order = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new GraphQLException($"Order {id} not found.");

            context.OrderItems.RemoveRange(order.Items);
            OrderDtoMapper.ApplyTo(order, input, status);

            await context.SaveChangesAsync();

            return OrderDtoMapper.ToDto(order);
        }

        public async Task<bool> DeleteOrder(
            int id,
            [Service] AppDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            RequireFullAccess(httpContextAccessor);

            var order = await context.Orders.FindAsync(id)
                ?? throw new GraphQLException($"Order {id} not found.");

            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            return true;
        }

        private static void RequireFullAccess(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user is null || !user.IsInRole("FullAccess"))
            {
                throw new GraphQLException("Forbidden: the FullAccess role is required for this mutation.");
            }
        }

        private static OrderStatus ParseStatus(string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, out var parsed))
            {
                throw new GraphQLException($"Invalid status '{status}'.");
            }

            return parsed;
        }
    }
}
