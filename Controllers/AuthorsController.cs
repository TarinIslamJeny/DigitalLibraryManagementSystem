using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class AuthorsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
        return View(authors);
    }

    [Authorize(Roles = "Admin,Librarian")]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin,Librarian")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Author model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Authors.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Author added successfully.";
        return RedirectToAction(nameof(Index));
    }
}
