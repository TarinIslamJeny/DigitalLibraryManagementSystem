using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class ReservationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "Member")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReserveBook(int bookId, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            var safeReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action(nameof(BooksController.Details), "Books", new { id = bookId }) ?? "/Books/Details/" + bookId;
            return RedirectToAction(nameof(AccountController.Login), "Account", new { returnUrl = safeReturnUrl });
        }

        var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
        if (book == null)
        {
            return NotFound();
        }

        if (book.AvailableCopies > 0)
        {
            TempData["ErrorMessage"] = "This book is currently available, please borrow it instead.";
            return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
        }

        var activeReservation = await _context.Reservations
            .AnyAsync(r => r.UserId == user.Id && r.BookId == bookId && r.Status != ReservationStatus.Completed && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Expired);

        if (activeReservation)
        {
            TempData["ErrorMessage"] = "This book is already reserved by you.";
            return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
        }

        var reservation = new Reservation
        {
            UserId = user.Id,
            BookId = bookId,
            ReservationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Status = ReservationStatus.Pending
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Book reserved successfully.";
        return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(int reservationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UserId == user.Id);
        if (reservation == null)
        {
            return NotFound();
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reservation cancelled.";
        return RedirectToAction(nameof(ProfileController.Index), "Profile");
    }
}
