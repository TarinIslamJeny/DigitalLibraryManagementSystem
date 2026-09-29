using System.Diagnostics;
using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using DigitalLibraryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .OrderByDescending(b => b.CreatedAt)
            .Take(6)
            .ToListAsync();

        var popularBooks = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .OrderByDescending(b => b.AvailableCopies)
            .Take(6)
            .ToListAsync();

        var digitalBooks = await _context.Books
            .Include(b => b.Author)
            .Where(b => b.IsDigitalAvailable && !string.IsNullOrWhiteSpace(b.DigitalFilePath))
            .OrderByDescending(b => b.CreatedAt)
            .Take(6)
            .ToListAsync();

        var categories = await _context.Categories.OrderBy(c => c.Name).Take(8).ToListAsync();
        var stats = new DashboardViewModel
        {
            TotalBooks = await _context.Books.CountAsync(),
            TotalMembers = await _context.Users.CountAsync(),
            AvailableBooks = await _context.Books.Where(b => b.AvailableCopies > 0).CountAsync(),
            DigitalBooks = await _context.Books.CountAsync(b => b.IsDigitalAvailable)
        };

        ViewBag.LatestBooks = books;
        ViewBag.PopularBooks = popularBooks;
        ViewBag.DigitalBooks = digitalBooks;
        ViewBag.Categories = categories;
        ViewBag.Stats = stats;

        return View(stats);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
