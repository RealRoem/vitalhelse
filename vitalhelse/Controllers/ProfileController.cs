/*using Microsoft.AspNetCore.Identity;
using VitalHelse.Data;
using VitalHelse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VitalHelse.Controllers;

public class ProfileController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductController> _logger;
    private readonly UserManager<AspNetUsers> _um;
    public ProfileController(ApplicationDbContext db, ILogger<ProductController> logger, UserManager<AspNetUsers> um)
    {
        _db = db;
        _logger = logger; 
        _um = um;
    }
    public IActionResult PrivateCustomer()
    {
        return View();
    }
    
} */