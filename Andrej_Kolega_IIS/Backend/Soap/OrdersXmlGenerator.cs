using System.Xml.Serialization;
using Andrej_Kolega_IIS.Backend.RestApi;

namespace Andrej_Kolega_IIS.Backend.Soap
{
    public class OrdersXmlGenerator
    {
        private readonly FirebaseOrdersClient _firebaseClient;
        private readonly string _xmlPath;

        public OrdersXmlGenerator(FirebaseOrdersClient firebaseClient, IWebHostEnvironment env)
        {
            _firebaseClient = firebaseClient;
            _xmlPath = System.IO.Path.Combine(env.ContentRootPath, "App_Data", "orders.xml");
        }

        public string XmlPath => _xmlPath;

        public async Task<int> GenerateAsync()
        {
            var orders = await _firebaseClient.FetchOrdersAsync();
            var xmlDto = OrderImportMapper.ToXmlDto(orders);

            var directory = System.IO.Path.GetDirectoryName(_xmlPath)!;
            Directory.CreateDirectory(directory);

            var serializer = new XmlSerializer(xmlDto.GetType());
            await using var writer = new StreamWriter(_xmlPath, append: false);
            serializer.Serialize(writer, xmlDto);

            return orders.Count;
        }
    }
}
