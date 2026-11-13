namespace ECommerceCsvImport.Repositories;

using ECommerceCsvImport.Models;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private readonly object _lock = new();

    public void Add(Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        lock (_lock)
        {
            _products.Add(product);
        }
    }

    public IReadOnlyList<Product> GetAll()
    {
        lock (_lock)
        {
            return _products.ToList();
        }
    }

    public Product? GetBySku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return null;
        }

        var normalizedSku = sku.Trim();

        lock (_lock)
        {
            return _products.FirstOrDefault(p =>
                string.Equals(p.Sku?.Trim(), normalizedSku, StringComparison.OrdinalIgnoreCase));
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _products.Count;
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _products.Clear();
        }
    }
}
