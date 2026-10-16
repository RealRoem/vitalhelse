using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

public class StockController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductController> _logger;
    private readonly UserManager<AspNetUsers> _um;
    
    public StockController(ApplicationDbContext db, ILogger<ProductController> logger,
        UserManager<AspNetUsers> um)
    {
        _db = db;
        _logger = logger;
        _um = um;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult StockCount()
    {
        var stockList = _db.Products
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .Select(p => new StockViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                StockCount = p.StockCount,
                ProductPriceInVAT = p.ProductPriceInVAT,
                FirstImagePath = p.ProductPictures.FirstOrDefault() != null 
                    ? p.ProductPictures.FirstOrDefault().PicturePath 
                    : null,
                CategoryNames = p.ProductCategories
                    .Select(pc => pc.Category.CategoryName)
                    .ToList()
            })
            .OrderBy(p => p.StockCount)
            .ToList();
    
        //gets all categories with parent/child structure
        var allCategories = _db.Categories
            .OrderBy(c => c.CategoryName)
            .ToList();
    
        ViewData["Categories"] = allCategories;
    
        return View("~/Views/Admin/StockCount.cshtml", stockList);
    }
    

    //test functionality for exporting to excel
    //dont know if were keeping this, so its bare minimum styling done by ai (not very pretty)
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult ExportStockToExcel()
    {
        var stockList = _db.Products
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Select(p => new
            {
                p.ProductId,
                p.ProductName,
                StockCount = p.StockCount ?? 0,
                p.ProductPriceInVAT,
                Categories = string.Join(", ", p.ProductCategories.Select(pc => pc.Category.CategoryName))
            })
            .OrderBy(p => p.StockCount)
            .ToList();
        
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Lager Beholdning");
            
            // Header row
            worksheet.Cell(1, 1).Value = "Produkt ID";
            worksheet.Cell(1, 2).Value = "Produkt Navn";
            worksheet.Cell(1, 3).Value = "Kategorier";
            worksheet.Cell(1, 4).Value = "Pris (inkl. MVA)";
            worksheet.Cell(1, 5).Value = "Beholdning";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Bestill Antall";
            
            // Style header
            var headerRow = worksheet.Range(1, 1, 1, 7);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37");
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            // Data rows
            int row = 2;
            foreach (var item in stockList)
            {
                worksheet.Cell(row, 1).Value = item.ProductId;
                worksheet.Cell(row, 2).Value = item.ProductName;
                worksheet.Cell(row, 3).Value = item.Categories;
                worksheet.Cell(row, 4).Value = item.ProductPriceInVAT;
                worksheet.Cell(row, 5).Value = item.StockCount;
                
                // Status
                string status;
                XLColor statusColor;
                if (item.StockCount <= 0)
                {
                    status = "Utsolgt";
                    statusColor = XLColor.FromHtml("#f8d7da");
                }
                else if (item.StockCount < 10)
                {
                    status = "Lav";
                    statusColor = XLColor.FromHtml("#fff3cd");
                }
                else
                {
                    status = "OK";
                    statusColor = XLColor.FromHtml("#d4edda");
                }
                
                worksheet.Cell(row, 6).Value = status;
                worksheet.Cell(row, 6).Style.Fill.BackgroundColor = statusColor;
                
                // Empty column for manual order quantity
                worksheet.Cell(row, 7).Value = "";
                
                row++;
            }
            
            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
            
            // Add borders to all cells
            var dataRange = worksheet.Range(1, 1, row - 1, 7);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            
            // Generate file
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                var fileName = $"Lager_Beholdning_{DateTime.Now:yyyy-MM-dd_HHmm}.xlsx";
                
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}