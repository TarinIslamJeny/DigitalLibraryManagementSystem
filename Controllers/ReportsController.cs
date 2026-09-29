using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? member, string? book)
    {
        var query = _context.BorrowRecords
            .Include(br => br.Book)
            .Include(br => br.User)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(br => br.BorrowDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(br => br.BorrowDate <= endDate.Value.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(member))
        {
            query = query.Where(br => br.User != null && br.User.FullName.Contains(member));
        }

        if (!string.IsNullOrWhiteSpace(book))
        {
            query = query.Where(br => br.Book != null && br.Book.Title.Contains(book));
        }

        var records = await query.OrderByDescending(br => br.BorrowDate).ToListAsync();
        ViewBag.Records = records;
        return View();
    }
}
