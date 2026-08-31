using System.Xml.Serialization;

namespace Andrej_Kolega_IIS.Backend.RestApi.Dto
{
    [XmlRoot("Orders")]
    public class OrdersXmlDto
    {
        [XmlElement("Order")]
        public List<OrderXmlDto> Orders { get; set; } = new();
    }

    public class OrderXmlDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;

        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<OrderItemXmlDto> Items { get; set; } = new();
    }

    public class OrderItemXmlDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
