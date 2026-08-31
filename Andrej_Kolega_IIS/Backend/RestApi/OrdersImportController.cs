using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Serialization;
using Andrej_Kolega_IIS.Backend.RestApi.Dto;
using Andrej_Kolega_IIS.Backend.RestApi.Validation;
using Andrej_Kolega_IIS.Shared.Data;
using Microsoft.AspNetCore.Mvc;

namespace Andrej_Kolega_IIS.Backend.RestApi
{
    [ApiController]
    [Route("api/rest/orders")]
    public class OrdersImportController : ControllerBase
    {
        private readonly OrderXmlValidator _xmlValidator;
        private readonly OrderJsonValidator _jsonValidator;
        private readonly AppDbContext _context;

        public OrdersImportController(OrderXmlValidator xmlValidator, OrderJsonValidator jsonValidator, AppDbContext context)
        {
            _xmlValidator = xmlValidator;
            _jsonValidator = jsonValidator;
            _context = context;
        }

        [HttpPost("xml")]
        public async Task<ActionResult<ImportResult>> ImportXml(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new ImportResult { Success = false, Errors = { "No file uploaded." } });
            }

            using var buffer = new MemoryStream();
            await using (var stream = file.OpenReadStream())
            {
                await stream.CopyToAsync(buffer);
            }
            buffer.Position = 0;

            var errors = _xmlValidator.Validate(buffer);
            if (errors.Count > 0)
            {
                return BadRequest(new ImportResult { Success = false, Errors = errors });
            }

            buffer.Position = 0;
            var serializer = new XmlSerializer(typeof(OrdersXmlDto));
            var xmlDto = (OrdersXmlDto?)serializer.Deserialize(buffer);
            if (xmlDto is null)
            {
                return BadRequest(new ImportResult { Success = false, Errors = { "Could not parse XML file." } });
            }

            var importDto = OrderImportMapper.ToImportDto(xmlDto);
            var entities = OrderImportMapper.ToEntities(importDto);

            _context.Orders.AddRange(entities);
            await _context.SaveChangesAsync();

            return Ok(new ImportResult { Success = true, SavedCount = entities.Count });
        }

        [HttpPost("json")]
        public async Task<ActionResult<ImportResult>> ImportJson(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new ImportResult { Success = false, Errors = { "No file uploaded." } });
            }

            using var buffer = new MemoryStream();
            await using (var stream = file.OpenReadStream())
            {
                await stream.CopyToAsync(buffer);
            }
            buffer.Position = 0;

            JsonNode? node;
            try
            {
                node = JsonNode.Parse(buffer);
            }
            catch (JsonException ex)
            {
                return BadRequest(new ImportResult { Success = false, Errors = { $"Malformed JSON: {ex.Message}" } });
            }

            var errors = _jsonValidator.Validate(node);
            if (errors.Count > 0)
            {
                return BadRequest(new ImportResult { Success = false, Errors = errors });
            }

            var importDto = node.Deserialize<OrdersImportDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (importDto is null)
            {
                return BadRequest(new ImportResult { Success = false, Errors = { "Could not parse JSON file." } });
            }

            var entities = OrderImportMapper.ToEntities(importDto);

            _context.Orders.AddRange(entities);
            await _context.SaveChangesAsync();

            return Ok(new ImportResult { Success = true, SavedCount = entities.Count });
        }
    }
}
