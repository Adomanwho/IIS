namespace Andrej_Kolega_IIS.Backend.RestApi.Dto
{
    public class OrdersImportDto
    {
        public List<OrderImportDto> Orders { get; set; } = new();
    }

    public class OrderImportDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public List<OrderItemImportDto> Items { get; set; } = new();
    }

    public class OrderItemImportDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
