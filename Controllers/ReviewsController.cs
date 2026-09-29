using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "Member")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int bookId, int rating, string? comment)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(AccountController.Login), "Account", new { returnUrl = Url.Action(nameof(BooksController.Details), "Books", new { id = bookId }) });
        }

        if (rating < 1 || rating > 5)
        {
            TempData["ErrorMessage"] = "Rating must be between 1 and 5.";
            return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["ErrorMessage"] = "Please enter a review comment.";
            return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
        }

        var existingReview = await _context.BookReviews.FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == user.Id);
        if (existingReview != null)
        {
            existingReview.Rating = rating;
            existingReview.Comment = comment.Trim();
            existingReview.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            var review = new BookReview
            {
                BookId = bookId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.BookReviews.Add(review);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Your review has been submitted.";
        return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
    }

    [Authorize(Roles = "Admin,Member")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var review = await _context.BookReviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        if (review == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var isOwner = review.UserId == user.Id;
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }

        var bookId = review.BookId;
        _context.BookReviews.Remove(review);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Review deleted.";
        return RedirectToAction(nameof(BooksController.Details), "Books", new { id = bookId });
    }
}
