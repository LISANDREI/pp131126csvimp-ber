using ECommerceCsvImport.Models;
using ECommerceCsvImport.Repositories;

namespace ECommerceCsvImport.Tests;

public class RepositoryTests
{
    [Fact]
    public void AddProduct_Should_Add_New_Product()
    {
        var repository = new InMemoryProductRepository();
        var product = new Product
        {
            Sku = "TEST001",
            Name = "Test Product",
            Price = 100
        };

        repository.Add(product);

        var all = repository.GetAll();
        Assert.Single(all);
        Assert.Equal("TEST001", all.First().Sku);
    }

    [Fact]
    public void GetAll_Should_Return_All_Products()
    {
        var repository = new InMemoryProductRepository();
        repository.Add(new Product { Sku = "SKU1", Name = "Product 1", Price = 100 });
        repository.Add(new Product { Sku = "SKU2", Name = "Product 2", Price = 200 });

        var result = repository.GetAll();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Clear_Should_Remove_All_Products()
    {
        var repository = new InMemoryProductRepository();
        repository.Add(new Product { Sku = "SKU1", Name = "Product 1", Price = 100 });

        repository.Clear();

        Assert.Empty(repository.GetAll());
    }
}
