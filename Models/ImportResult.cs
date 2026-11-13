namespace ECommerceCsvImport.Models
{
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
        public int ErrorCount => Errors.Count;
    }
}
