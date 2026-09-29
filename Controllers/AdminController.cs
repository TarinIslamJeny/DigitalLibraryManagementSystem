using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using DigitalLibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DashboardService _dashboardService;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, DashboardService dashboardService)
    {
        _context = context;
        _userManager = userManager;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await _dashboardService.GetDashboardSummaryAsync();
        ViewBag.RecentBorrowings = await _context.BorrowRecords
            .Include(br => br.Book)
            .Include(br => br.User)
            .OrderByDescending(br => br.BorrowDate)
            .Take(5)
            .ToListAsync();

        ViewBag.OverdueBooks = await _context.BorrowRecords
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.Status == BorrowStatus.Overdue || (br.Status == BorrowStatus.Borrowed && br.DueDate < DateTime.UtcNow))
            .OrderBy(br => br.DueDate)
            .Take(10)
            .ToListAsync();

        ViewBag.RecentMembers = await _context.Users
            .OrderByDescending(u => u.RegistrationDate)
            .Take(5)
            .ToListAsync();

        ViewBag.Reservations = await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.User)
            .OrderByDescending(r => r.ReservationDate)
            .Take(5)
            .ToListAsync();

        return View(model);
    }

    public async Task<IActionResult> Members()
    {
        var members = await _context.Users
            .OrderByDescending(u => u.RegistrationDate)
            .ToListAsync();

        return View(members);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = $"Membership status updated for {user.FullName}.";
        return RedirectToAction(nameof(Members));
    }

    public IActionResult Reports() => RedirectToAction(nameof(ReportsController.Index), "Reports");

    [HttpGet]
    public IActionResult ViewMember(string id)
    {
        var member = _context.Users.FirstOrDefault(u => u.Id == id);
        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    [HttpGet]
    public async Task<IActionResult> BorrowHistory(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }

        var history = await _context.BorrowRecords
            .Include(br => br.Book)
            .Where(br => br.UserId == userId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync();

        ViewBag.UserName = user.FullName;
        return View(history);
    }
}
