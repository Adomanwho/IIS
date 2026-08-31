using System.ServiceModel;
using Andrej_Kolega_IIS.Backend.Soap.Dto;

namespace Andrej_Kolega_IIS.Backend.Soap
{
    [ServiceContract]
    public interface IOrdersSoapService
    {
        [OperationContract]
        Task<GenerateXmlResult> GenerateOrdersXml();

        [OperationContract]
        Task<SearchOrdersResult> SearchOrdersByCustomerName(string customerNameTerm);
    }
}
