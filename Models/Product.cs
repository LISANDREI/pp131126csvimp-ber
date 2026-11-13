namespace ECommerceCsvImport.Models;

public class Product
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Brand { get; set; } = string.Empty;
    public int WarrantyMonths { get; set; }
    public string ModelNumber { get; set; } = string.Empty;
}
