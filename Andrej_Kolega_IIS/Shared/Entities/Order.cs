namespace Andrej_Kolega_IIS.Shared.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string ShippingCity { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
    }
}
