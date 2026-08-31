using Andrej_Kolega_IIS.Backend.Soap.Dto;

namespace Andrej_Kolega_IIS.Frontend.Models
{
    public class OrdersSoapSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public SearchOrdersResult? Result { get; set; }
    }
}
