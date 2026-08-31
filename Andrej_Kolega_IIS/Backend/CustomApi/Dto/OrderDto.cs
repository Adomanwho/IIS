using System.ComponentModel.DataAnnotations;

namespace Andrej_Kolega_IIS.Backend.CustomApi.Dto
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderWriteDto
    {
        [Required, MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ShippingCity { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public List<OrderItemWriteDto> Items { get; set; } = new();
    }

    public class OrderItemWriteDto
    {
        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
