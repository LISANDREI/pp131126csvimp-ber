using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests;

public class CsvParserTests
{
    [Fact]
    public void Parse_Simple_Line_Should_Return_Correct_Fields()
    {
        var line = "1001,Product,Description,1000,RUB,Category,10,/image.jpg,true,Brand,12,Model";
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal(12, fields.Count);
        Assert.Equal("1001", fields[0]);
        Assert.Equal("Product", fields[1]);
        Assert.Equal("Brand", fields[9]);
    }

    [Fact]
    public void Parse_Line_With_Quotes_Should_Handle_Commas()
    {
        var line = "1001,\"Product, with comma\",Description,1000,RUB,Category,10,/image.jpg,true,Brand,12,Model";
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal("Product, with comma", fields[1]);
    }

    [Fact]
    public void Parse_Empty_Fields_Should_Return_Empty_Strings()
    {
        var line = "1001,Product,,1000,RUB,Category,10,,true,Brand,12,";
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal("", fields[2]);
        Assert.Equal("", fields[7]);
        Assert.Equal("", fields[11]);
    }

    [Fact]
    public void Parse_Cyrillic_Should_Work()
    {
        var line = "1001,Ноутбук Lenovo,Бюджетный ноутбук,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad";
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal(12, fields.Count);
        Assert.Equal("Ноутбук Lenovo", fields[1]);
        Assert.Equal("Lenovo", fields[9]);
    }
}
