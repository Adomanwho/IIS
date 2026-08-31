namespace Andrej_Kolega_IIS.Backend.RestApi.Dto
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public int SavedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
