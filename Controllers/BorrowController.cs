using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class BorrowController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BorrowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "Member")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BorrowBook(int bookId, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            var safeReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action(nameof(BooksController.Details), "Books", new { id = bookId }) ?? "/Books/Details/" + bookId;
            return RedirectToAction(nameof(AccountController.Login), "Account", new { returnUrl = safeReturnUrl });
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Book not found.";
                    throw new InvalidOperationException("Book not found.");
                }

                var existingBorrow = await _context.BorrowRecords
                    .AnyAsync(br => br.UserId == user.Id && br.BookId == bookId && br.Status != BorrowStatus.Returned);

                if (existingBorrow)
                {
                    TempData["ErrorMessage"] = "This book is already borrowed by you.";
                    throw new InvalidOperationException("This book is already borrowed by you.");
                }

                var activeBorrows = await _context.BorrowRecords.CountAsync(br => br.UserId == user.Id && br.Status != BorrowStatus.Returned);
                if (activeBorrows >= 5)
                {
                    TempData["ErrorMessage"] = "You have reached the maximum limit of 5 books.";
                    throw new InvalidOperationException("You have reached the maximum limit of 5 books.");
                }

                if (book.AvailableCopies <= 0)
                {
                    TempData["ErrorMessage"] = "No copies are currently available.";
                    throw new InvalidOperationException("No copies are currently available.");
                }

                var borrowDate = DateTime.UtcNow;
                var borrowRecord = new BorrowRecord
                {
                    UserId = user.Id,
                    BookId = book.BookId,
                    BorrowDate = borrowDate,
                    DueDate = borrowDate.AddDays(14),
                    Status = BorrowStatus.Borrowed,
                    FineAmount = 0,
                    IsFinePaid = false
                };

                _context.BorrowRecords.Add(borrowRecord);
                book.AvailableCopies -= 1;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            });
        }
        catch (Exception)
        {
            if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "Borrowing failed. Please try again.";
            }

            return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
        }

        TempData["SuccessMessage"] = "Book borrowed successfully.";
        return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnBook(int borrowRecordId)
    {
        var borrowRecord = await _context.BorrowRecords
            .Include(br => br.Book)
            .FirstOrDefaultAsync(br => br.BorrowRecordId == borrowRecordId);

        if (borrowRecord == null)
        {
            return NotFound();
        }

        if (borrowRecord.ReturnDate.HasValue)
        {
            TempData["ErrorMessage"] = "This borrow record has already been returned.";
            return RedirectToAction(nameof(ProfileController.Index), "Profile");
        }

        var returnDate = DateTime.UtcNow;
        borrowRecord.ReturnDate = returnDate;
        borrowRecord.Status = BorrowStatus.Returned;

        if (returnDate > borrowRecord.DueDate)
        {
            var overdueDays = (returnDate - borrowRecord.DueDate).Days;
            var fineAmount = overdueDays * 10m;
            borrowRecord.FineAmount = fineAmount;
            borrowRecord.IsFinePaid = false;

            var fine = new Fine
            {
                UserId = borrowRecord.UserId,
                BorrowRecordId = borrowRecord.BorrowRecordId,
                Amount = fineAmount,
                Reason = "Overdue return fine",
                CreatedDate = returnDate,
                Status = FineStatus.Unpaid
            };

            _context.Fines.Add(fine);
        }

        if (borrowRecord.Book != null)
        {
            borrowRecord.Book.AvailableCopies += 1;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Book returned successfully.";
        return RedirectToAction(nameof(ProfileController.Index), "Profile");
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> OverdueBooks()
    {
        var overdue = await _context.BorrowRecords
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.Status == BorrowStatus.Overdue || (br.Status == BorrowStatus.Borrowed && br.DueDate < DateTime.UtcNow))
            .OrderBy(br => br.DueDate)
            .ToListAsync();

        return View(overdue);
    }
}
