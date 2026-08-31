using System.Xml;
using System.Xml.Schema;

namespace Andrej_Kolega_IIS.Backend.RestApi.Validation
{
    public class OrderXmlValidator
    {
        private readonly string _xsdPath;

        public OrderXmlValidator(IWebHostEnvironment env)
        {
            _xsdPath = System.IO.Path.Combine(env.ContentRootPath, "Shared", "Schemas", "order.xsd");
        }

        public List<string> Validate(Stream xmlStream)
        {
            var errors = new List<string>();

            var schemas = new XmlSchemaSet();
            schemas.Add(null, _xsdPath);

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            };
            settings.ValidationEventHandler += (_, e) => errors.Add(e.Message);

            try
            {
                using var reader = XmlReader.Create(xmlStream, settings);
                while (reader.Read())
                {
                }
            }
            catch (XmlException ex)
            {
                errors.Add($"Malformed XML: {ex.Message}");
            }

            return errors;
        }
    }
}
