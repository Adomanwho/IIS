using System.ComponentModel.DataAnnotations;

namespace Andrej_Kolega_IIS.Frontend.Models
{
    public class OrderFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public string ShippingCity { get; set; } = string.Empty;

        public List<OrderItemFormRow> Items { get; set; } = new()
        {
            new OrderItemFormRow(),
            new OrderItemFormRow(),
            new OrderItemFormRow()
        };

        public string? ErrorMessage { get; set; }
    }

    public class OrderItemFormRow
    {
        // Nullable so a blank row (used to "skip" an item slot in the form) does not fail
        // MVC's implicit-required validation for non-nullable reference type properties.
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
