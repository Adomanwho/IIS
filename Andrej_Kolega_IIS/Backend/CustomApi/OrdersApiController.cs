using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Shared.Data;
using Andrej_Kolega_IIS.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Andrej_Kolega_IIS.Backend.CustomApi
{
    [ApiController]
    [Route("api/custom/orders")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAll()
        {
            var orders = await _context.Orders.Include(o => o.Items).ToListAsync();
            return Ok(orders.Select(OrderDtoMapper.ToDto).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null)
            {
                return NotFound();
            }

            return Ok(OrderDtoMapper.ToDto(order));
        }

        [HttpPost]
        [Authorize(Roles = "FullAccess")]
        public async Task<ActionResult<OrderDto>> Create(OrderWriteDto request)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, out var status))
            {
                return BadRequest(new { message = $"Invalid status '{request.Status}'." });
            }

            var order = new Order();
            OrderDtoMapper.ApplyTo(order, request, status);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, OrderDtoMapper.ToDto(order));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "FullAccess")]
        public async Task<ActionResult<OrderDto>> Update(int id, OrderWriteDto request)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, out var status))
            {
                return BadRequest(new { message = $"Invalid status '{request.Status}'." });
            }

            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null)
            {
                return NotFound();
            }

            _context.OrderItems.RemoveRange(order.Items);
            OrderDtoMapper.ApplyTo(order, request, status);

            await _context.SaveChangesAsync();

            return Ok(OrderDtoMapper.ToDto(order));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
