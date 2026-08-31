using System.Runtime.Serialization;

namespace Andrej_Kolega_IIS.Backend.Soap.Dto
{
    [DataContract]
    public class GenerateXmlResult
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public int OrderCount { get; set; }

        [DataMember]
        public string? ErrorMessage { get; set; }
    }

    [DataContract]
    public class SearchOrdersResult
    {
        [DataMember]
        public bool ValidationPassed { get; set; }

        [DataMember]
        public List<string> ValidationErrors { get; set; } = new();

        [DataMember]
        public List<OrderSearchItem> Orders { get; set; } = new();
    }

    [DataContract]
    public class OrderSearchItem
    {
        [DataMember]
        public string CustomerName { get; set; } = string.Empty;

        [DataMember]
        public string CustomerEmail { get; set; } = string.Empty;

        [DataMember]
        public string OrderDate { get; set; } = string.Empty;

        [DataMember]
        public string Status { get; set; } = string.Empty;

        [DataMember]
        public string ShippingCity { get; set; } = string.Empty;

        [DataMember]
        public List<OrderSearchItemLine> Items { get; set; } = new();
    }

    [DataContract]
    public class OrderSearchItemLine
    {
        [DataMember]
        public string ProductName { get; set; } = string.Empty;

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public decimal UnitPrice { get; set; }
    }
}
