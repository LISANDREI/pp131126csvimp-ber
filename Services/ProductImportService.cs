namespace ECommerceCsvImport.Services;

using ECommerceCsvImport.Models;
using ECommerceCsvImport.Repositories;
using System.Globalization;

public class ProductImportService : IProductImportService
{
    private readonly IProductRepository _repository;

    public ProductImportService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ImportResult> ImportFromCsvAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new ImportResult
            {
                TotalRows = 0,
                ImportedCount = 0,
                Errors = new List<ImportError>
                {
                    new ImportError { RowNumber = 0, Message = "Файл не найден" }
                }
            };
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        return await ImportFromLinesAsync(lines);
    }

    public Task<ImportResult> ImportFromLinesAsync(IEnumerable<string> lines)
    {
        var result = new ImportResult();
        
        if (lines == null || !lines.Any())
        {
            result.Errors.Add(new ImportError { RowNumber = 0, Message = "Данные для импорта отсутствуют" });
            return Task.FromResult(result);
        }

        var linesList = lines.ToList();
        
        string? headerLine = null;
        int headerIndex = -1;
        for (int i = 0; i < linesList.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(linesList[i]))
            {
                headerLine = linesList[i];
                headerIndex = i;
                break;
            }
        }

        if (headerLine == null)
        {
            result.Errors.Add(new ImportError { RowNumber = 0, Message = "Данные для импорта отсутствуют" });
            return Task.FromResult(result);
        }

        var headerFields = CsvLineParser.Parse(headerLine);
        var requiredColumns = new[] { "sku", "name", "description", "price", "currency", 
            "category_name", "stock", "image_url", "is_active", "brand", "warranty_months", "model_number" };

        if (headerFields.Count != 12)
        {
            result.Errors.Add(new ImportError { RowNumber = 0, Message = "Неверные заголовки CSV" });
            return Task.FromResult(result);
        }

        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headerFields.Count; i++)
            columnMap[headerFields[i]] = i;

        foreach (var col in requiredColumns)
        {
            if (!columnMap.ContainsKey(col))
            {
                result.Errors.Add(new ImportError { RowNumber = 0, Message = "Неверные заголовки CSV" });
                return Task.FromResult(result);
            }
        }

        bool hasData = linesList.Skip(headerIndex + 1).Any(line => !string.IsNullOrWhiteSpace(line));
        if (!hasData)
        {
            result.Errors.Add(new ImportError { RowNumber = 0, Message = "Данные для импорта отсутствуют" });
            return Task.FromResult(result);
        }

        var importedSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = headerIndex + 1; i < linesList.Count; i++)
        {
            var line = linesList[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            result.TotalRows++;
            int rowNum = result.TotalRows + 1;

            var fields = CsvLineParser.Parse(line);
            if (fields.Count != 12)
            {
                result.Errors.Add(new ImportError { RowNumber = rowNum, Message = "Неверное количество полей в строке" });
                continue;
            }

            string GetValue(string colName) => columnMap.TryGetValue(colName, out int idx) && idx < fields.Count ? fields[idx].Trim() : string.Empty;

            var rowErrors = new List<ImportError>();

            var sku = GetValue("sku");
            if (string.IsNullOrWhiteSpace(sku))
                rowErrors.Add(new ImportError { RowNumber = rowNum, Message = "Поле sku не может быть пустым" });

            var name = GetValue("name");
            if (string.IsNullOrWhiteSpace(name))
                rowErrors.Add(new ImportError { RowNumber = rowNum, Message = "Поле name не может быть пустым" });

            var brand = GetValue("brand");
            if (string.IsNullOrWhiteSpace(brand))
                rowErrors.Add(new ImportError { RowNumber = rowNum, Message = "Поле brand обязательно для варианта «Электроника»" });

            var priceStr = GetValue("price");
            if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price <= 0)
                rowErrors.Add(new ImportError { RowNumber = rowNum, Message = "Поле price должно быть числом больше 0" });

            var stockStr = GetValue("stock");
            if (!int.TryParse(stockStr, out int stock) || stock < 0)
                rowErrors.Add(new ImportError { RowNumber = rowNum, Message = "Остаток на складе должен быть целым числом не меньше 0" });

            var isActiveStr = GetValue("is_active");
            bool isActive = false;
            if (!TryParseBool(isActiveStr, out isActive))
                rowErrors.Add(new ImportError { RowNumber = rowNum, Message = "Поле is_active должно иметь значение true, false, 1 или 0" });

            var warrantyStr = GetValue("warranty_months");
            if (!int.TryParse(warrantyStr, out int warranty) || warranty < 0 || warranty > 60)
                rowErrors.Add(new ImportError { RowNumber = rowNum, Message = "Гарантия должна быть целым числом от 0 до 60 месяцев" });

            if (rowErrors.Count > 0)
            {
                result.Errors.AddRange(rowErrors);
                continue;
            }

            if (_repository.GetBySku(sku) != null || importedSkus.Contains(sku))
            {
                result.Errors.Add(new ImportError { RowNumber = rowNum, Message = "Артикул товара уже существует" });
                continue;
            }

            var product = new Product
            {
                Sku = sku,
                Name = name,
                Description = GetValue("description"),
                Price = price,
                Currency = NormalizeCurrency(GetValue("currency")),
                CategoryName = NormalizeCategory(GetValue("category_name")),
                StockQuantity = stock,
                ImageUrl = GetValue("image_url"),
                IsActive = isActive,
                Brand = brand,
                WarrantyMonths = warranty,
                ModelNumber = GetValue("model_number")
            };

            _repository.Add(product);
            importedSkus.Add(sku);
            result.ImportedCount++;
        }

        return Task.FromResult(result);
    }

    private static bool TryParseBool(string value, out bool result)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized == "true" || normalized == "1")
        {
            result = true;
            return true;
        }
        if (normalized == "false" || normalized == "0")
        {
            result = false;
            return true;
        }
        result = false;
        return false;
    }

    private static string NormalizeCurrency(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "RUB" : value.Trim().ToUpperInvariant();
    }

    private static string NormalizeCategory(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Без категории" : value.Trim();
    }
}
