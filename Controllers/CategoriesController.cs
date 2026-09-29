using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(categories);
    }

    [Authorize(Roles = "Admin,Librarian")]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin,Librarian")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Categories.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Category added successfully.";
        return RedirectToAction(nameof(Index));
    }
}
