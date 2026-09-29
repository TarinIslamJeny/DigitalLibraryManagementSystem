using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

[Authorize(Roles = "Admin,Librarian")]
public class LibrarianController : Controller
{
    private readonly ApplicationDbContext _context;

    public LibrarianController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.UtcNow;
        var model = new
        {
            TotalBooks = await _context.Books.CountAsync(),
            CurrentlyBorrowed = await _context.BorrowRecords.CountAsync(br => br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue),
            BooksDueToday = await _context.BorrowRecords.CountAsync(br => br.Status == BorrowStatus.Borrowed && br.DueDate.Date == today.Date),
            OverdueBooks = await _context.BorrowRecords.CountAsync(br => br.Status == BorrowStatus.Overdue || (br.Status == BorrowStatus.Borrowed && br.DueDate < today)),
            PendingReservations = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending)
        };

        return View(model);
    }

    public IActionResult IssueBook()
    {
        return View();
    }

    public IActionResult ReturnBook()
    {
        return View();
    }

    public async Task<IActionResult> SearchMember(string query)
    {
        var members = string.IsNullOrWhiteSpace(query)
            ? new List<ApplicationUser>()
            : await _context.Users
                .Where(u => u.FullName.Contains(query) || (u.Email != null && u.Email.Contains(query)))
                .OrderBy(u => u.FullName)
                .Take(20)
                .ToListAsync();

        return View(members);
    }

    public async Task<IActionResult> SearchBooks(string query)
    {
        var books = string.IsNullOrWhiteSpace(query)
            ? await _context.Books.Include(b => b.Author).Include(b => b.Category).OrderBy(b => b.Title).Take(20).ToListAsync()
            : await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Title.Contains(query)
                    || b.ISBN.Contains(query)
                    || (b.Author != null && b.Author.Name != null && b.Author.Name.Contains(query)))
                .OrderBy(b => b.Title)
                .Take(20)
                .ToListAsync();

        return View(books);
    }
}
