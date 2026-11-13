using ECommerceCsvImport.Models;
using ECommerceCsvImport.Repositories;
using ECommerceCsvImport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _repository;
    private readonly IProductImportService _importService;
    private readonly IWebHostEnvironment _environment;

    public ProductsController(
        IProductRepository repository,
        IProductImportService importService,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _importService = importService;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var viewModel = new ProductsIndexViewModel
        {
            Products = _repository.GetAll()
        };
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View(new ImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile? uploadedFile, string? filePath)
    {
        ImportResult result;
        string displayPath = string.Empty;

        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            
            var fileName = Path.GetFileName(uploadedFile.FileName);
            var tempFilePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(stream);
            }
            
            result = await _importService.ImportFromCsvAsync(tempFilePath);
            displayPath = fileName;
            
            try { System.IO.File.Delete(tempFilePath); } catch { }
        }
        else if (!string.IsNullOrWhiteSpace(filePath))
        {
            var resolvedPath = ResolveFilePath(filePath);
            result = await _importService.ImportFromCsvAsync(resolvedPath);
            displayPath = filePath;
        }
        else
        {
            return View(new ImportViewModel
            {
                FilePath = string.Empty,
                Result = new ImportResult
                {
                    Errors = { new ImportError { RowNumber = 0, Message = "Выберите файл или укажите путь к CSV-файлу" } }
                }
            });
        }

        return View(new ImportViewModel
        {
            FilePath = displayPath,
            Result = result
        });
    }

    private string ResolveFilePath(string input)
    {
        var trimmed = input.Trim();
        if (Path.IsPathRooted(trimmed))
            return trimmed;

        if (trimmed.StartsWith("/"))
            trimmed = trimmed.TrimStart('/');

        return Path.Combine(_environment.WebRootPath, trimmed);
    }
}
