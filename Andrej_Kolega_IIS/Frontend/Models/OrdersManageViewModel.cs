using Andrej_Kolega_IIS.Backend.CustomApi.Dto;
using Andrej_Kolega_IIS.Backend.RestApi.Dto;

namespace Andrej_Kolega_IIS.Frontend.Models
{
    public class OrdersManageViewModel
    {
        public string Mode { get; set; } = "Public";
        public List<OrderDto>? CustomOrders { get; set; }
        public List<OrderImportDto>? PublicOrders { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
