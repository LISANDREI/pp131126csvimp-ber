using ECommerceCsvImport.Services;
using ECommerceCsvImport.Repositories;

namespace ECommerceCsvImport.Tests;

public class ImportServiceTests
{
    private const string Header = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number";

    [Fact]
    public async Task Import_Valid_CSV_Should_Succeed()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository);
        var lines = new[]
        {
            Header,
            "1001,Ноутбук Lenovo,Бюджетный ноутбук,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3"
        };

        var result = await service.ImportFromLinesAsync(lines);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(1, result.ImportedCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Import_Negative_Price_Should_Fail()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository);
        var lines = new[]
        {
            Header,
            "1003,Клавиатура,Механическая,-500,RUB,Периферия,3,/images/keyboard.jpg,true,A4Tech,24,V3M"
        };

        var result = await service.ImportFromLinesAsync(lines);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("price"));
    }

    [Fact]
    public async Task Import_Duplicate_SKU_Should_Fail()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository);
        var lines = new[]
        {
            Header,
            "1001,Ноутбук Lenovo,Бюджетный,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad",
            "1001,Ноутбук ASUS,Другой,52990,RUB,Компьютеры,7,/images/asus.jpg,true,ASUS,24,VivoBook"
        };

        var result = await service.ImportFromLinesAsync(lines);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.ImportedCount);
        Assert.Contains(result.Errors, e => e.Message.Contains("Артикул"));
    }

    [Fact]
    public async Task Import_Empty_File_Should_Return_Error()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository);
        var lines = new[] { Header };

        var result = await service.ImportFromLinesAsync(lines);

        Assert.Equal(0, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, e => e.Message.Contains("отсутствуют"));
    }

    [Fact]
    public async Task Import_Negative_Stock_Should_Fail()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository);
        var lines = new[]
        {
            Header,
            "1004,Монитор Dell,Игровой монитор,29990,RUB,Компьютеры,-5,/images/monitor.jpg,true,Dell,24,S2722QC"
        };

        var result = await service.ImportFromLinesAsync(lines);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, e => e.Message.Contains("Остаток"));
    }

    [Fact]
    public async Task Import_Missing_Brand_Should_Fail()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository);
        var lines = new[]
        {
            Header,
            "1005,Видеокарта NVIDIA,Графическая карта,24990,RUB,Компьютеры,4,/images/gpu.jpg,true,,36,RTX3060"
        };

        var result = await service.ImportFromLinesAsync(lines);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, e => e.Message.Contains("brand"));
    }

    [Fact]
    public async Task Import_Warranty_Out_Of_Range_Should_Fail()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository);
        var lines = new[]
        {
            Header,
            "1006,Накопитель SSD,Твердотельный,5990,RUB,Компьютеры,12,/images/ssd.jpg,true,Samsung,61,870EVO"
        };

        var result = await service.ImportFromLinesAsync(lines);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, e => e.Message.Contains("Гарантия"));
    }
}
