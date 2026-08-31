using System.Text.Json.Nodes;
using Json.Schema;

namespace Andrej_Kolega_IIS.Backend.RestApi.Validation
{
    public class OrderJsonValidator
    {
        private readonly JsonSchema _schema;

        public OrderJsonValidator(IWebHostEnvironment env)
        {
            var path = System.IO.Path.Combine(env.ContentRootPath, "Shared", "Schemas", "order.schema.json");
            _schema = JsonSchema.FromText(File.ReadAllText(path));
        }

        public List<string> Validate(JsonNode? node)
        {
            var errors = new List<string>();

            var results = _schema.Evaluate(node, new EvaluationOptions { OutputFormat = OutputFormat.List });
            if (!results.IsValid)
            {
                CollectErrors(results, errors);
            }

            return errors;
        }

        private static void CollectErrors(EvaluationResults results, List<string> errors)
        {
            if (results.HasErrors)
            {
                foreach (var (keyword, message) in results.Errors!)
                {
                    errors.Add($"{results.InstanceLocation}: {message}");
                }
            }

            foreach (var detail in results.Details)
            {
                CollectErrors(detail, errors);
            }
        }
    }
}
