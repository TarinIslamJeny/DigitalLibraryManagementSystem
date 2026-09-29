using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var borrowHistory = await _context.BorrowRecords
            .Include(br => br.Book)
            .Where(br => br.UserId == user.Id)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync();

        var activeReservations = await _context.Reservations
            .Include(r => r.Book)
            .Where(r => r.UserId == user.Id && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Completed && r.Status != ReservationStatus.Expired)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();

        var notifications = await _context.Notifications
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .ToListAsync();

        var fines = await _context.Fines
            .Where(f => f.UserId == user.Id)
            .OrderByDescending(f => f.CreatedDate)
            .ToListAsync();

        ViewBag.BorrowHistory = borrowHistory;
        ViewBag.ActiveReservations = activeReservations;
        ViewBag.Notifications = notifications;
        ViewBag.Fines = fines;
        return View(user);
    }

    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ApplicationUser model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
