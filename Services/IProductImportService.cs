namespace ECommerceCsvImport.Services;

using ECommerceCsvImport.Models;

public interface IProductImportService
{
    Task<ImportResult> ImportFromCsvAsync(string filePath);
    Task<ImportResult> ImportFromLinesAsync(IEnumerable<string> lines);
}
