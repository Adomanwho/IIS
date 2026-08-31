using Andrej_Kolega_IIS.Backend.RestApi.Dto;

namespace Andrej_Kolega_IIS.Frontend.Models
{
    public class OrderImportViewModel
    {
        public IFormFile? File { get; set; }
        public string? ErrorMessage { get; set; }
        public ImportResult? Result { get; set; }
    }
}
