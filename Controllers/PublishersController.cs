using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class PublishersController : Controller
{
    private readonly ApplicationDbContext _context;

    public PublishersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var publishers = await _context.Publishers.OrderBy(p => p.Name).ToListAsync();
        return View(publishers);
    }

    [Authorize(Roles = "Admin,Librarian")]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin,Librarian")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Publisher model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Publishers.Add(model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Publisher added successfully.";
        return RedirectToAction(nameof(Index));
    }
}
