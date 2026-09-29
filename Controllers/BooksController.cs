using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using DigitalLibraryManagementSystem.Services;
using DigitalLibraryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Controllers;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly FileStorageService _fileStorageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BooksController(ApplicationDbContext context, FileStorageService fileStorageService, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(BookFilterViewModel? filter)
    {
        filter ??= new BookFilterViewModel();

        var query = _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Publisher)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim();
            query = query.Where(b =>
                b.Title.Contains(search) ||
                b.ISBN.Contains(search) ||
                (b.Author != null && b.Author.Name.Contains(search)) ||
                (b.Category != null && b.Category.Name.Contains(search)) ||
                (b.Publisher != null && b.Publisher.Name.Contains(search)));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == filter.CategoryId.Value);
        }

        if (filter.AuthorId.HasValue)
        {
            query = query.Where(b => b.AuthorId == filter.AuthorId.Value);
        }

        if (filter.Availability != "All")
        {
            query = filter.Availability switch
            {
                "Available" => query.Where(b => b.AvailableCopies > 0),
                "Unavailable" => query.Where(b => b.AvailableCopies == 0),
                _ => query
            };
        }

        if (filter.DigitalAvailability != "All")
        {
            query = filter.DigitalAvailability switch
            {
                "Digital" => query.Where(b => b.IsDigitalAvailable && !string.IsNullOrWhiteSpace(b.DigitalFilePath)),
                "NonDigital" => query.Where(b => !b.IsDigitalAvailable || string.IsNullOrWhiteSpace(b.DigitalFilePath)),
                _ => query
            };
        }

        if (filter.PublishedYear.HasValue)
        {
            query = query.Where(b => b.PublishedYear == filter.PublishedYear.Value);
        }

        query = filter.SortBy switch
        {
            "TitleDesc" => query.OrderByDescending(b => b.Title),
            "Newest" => query.OrderByDescending(b => b.PublishedYear),
            "Oldest" => query.OrderBy(b => b.PublishedYear),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();
        var books = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
        ViewBag.PageNumber = filter.PageNumber;

        return View(books);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Publisher)
            .Include(b => b.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null)
        {
            return NotFound();
        }

        var averageRating = book.Reviews.Any() ? book.Reviews.Average(r => r.Rating) : 0;
        ViewBag.AverageRating = averageRating;
        ViewBag.ReviewCount = book.Reviews.Count;
        return View(book);
    }

    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Create()
    {
        var viewModel = await BuildBookFormViewModelAsync();
        return View("Edit", viewModel);
    }

    [Authorize(Roles = "Admin,Librarian")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(60 * 1024 * 1024)]
    public async Task<IActionResult> Create(BookFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateFormOptionsAsync(model);
            return View("Edit", model);
        }

        var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == model.ISBN);
        if (existingBook != null)
        {
            ModelState.AddModelError(nameof(model.ISBN), "A book with this ISBN already exists.");
            await PopulateFormOptionsAsync(model);
            return View("Edit", model);
        }

        string? coverPath = null;
        string? digitalPath = null;
        try
        {
            coverPath = await _fileStorageService.SaveCoverImageAsync(model.CoverImage);
            digitalPath = await _fileStorageService.SaveDigitalBookAsync(model.DigitalFile);
        }
        catch (InvalidOperationException exception)
        {
            _fileStorageService.DeleteCoverIfOwned(coverPath);
            _fileStorageService.DeleteDigitalBookIfOwned(digitalPath);
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateFormOptionsAsync(model);
            return View("Edit", model);
        }
        catch
        {
            _fileStorageService.DeleteCoverIfOwned(coverPath);
            _fileStorageService.DeleteDigitalBookIfOwned(digitalPath);
            throw;
        }

        var book = new Book
        {
            Title = model.Title,
            ISBN = model.ISBN,
            Description = model.Description,
            AuthorId = model.AuthorId,
            CategoryId = model.CategoryId,
            PublisherId = model.PublisherId,
            PublishedYear = model.PublishedYear,
            Edition = model.Edition,
            Language = model.Language,
            TotalCopies = model.TotalCopies,
            AvailableCopies = model.TotalCopies,
            ShelfLocation = model.ShelfLocation,
            CoverImagePath = coverPath,
            DigitalFilePath = digitalPath,
            IsDigitalAvailable = _fileStorageService.DigitalFileExists(digitalPath),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Books.Add(book);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            _fileStorageService.DeleteCoverIfOwned(coverPath);
            _fileStorageService.DeleteDigitalBookIfOwned(digitalPath);
            throw;
        }

        TempData["SuccessMessage"] = "Book created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book == null)
        {
            return NotFound();
        }

        var viewModel = new BookFormViewModel
        {
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            Description = book.Description,
            AuthorId = book.AuthorId,
            CategoryId = book.CategoryId,
            PublisherId = book.PublisherId,
            PublishedYear = book.PublishedYear,
            Edition = book.Edition,
            Language = book.Language,
            TotalCopies = book.TotalCopies,
            ShelfLocation = book.ShelfLocation,
            ExistingCoverImagePath = book.CoverImagePath,
            ExistingDigitalFilePath = book.DigitalFilePath
        };

        await PopulateFormOptionsAsync(viewModel);
        return View(viewModel);
    }

    [Authorize(Roles = "Admin,Librarian")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(60 * 1024 * 1024)]
    public async Task<IActionResult> Edit(int id, BookFormViewModel model)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.BookId = id;
            model.ExistingCoverImagePath = book.CoverImagePath;
            model.ExistingDigitalFilePath = book.DigitalFilePath;
            await PopulateFormOptionsAsync(model);
            return View(model);
        }

        var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == model.ISBN && b.BookId != id);
        if (existingBook != null)
        {
            ModelState.AddModelError(nameof(model.ISBN), "A book with this ISBN already exists.");
            await PopulateFormOptionsAsync(model);
            return View(model);
        }

        var previousCoverPath = book.CoverImagePath;
        var previousDigitalPath = book.DigitalFilePath;
        string? uploadedCoverPath = null;
        string? uploadedDigitalPath = null;
        try
        {
            uploadedCoverPath = await _fileStorageService.SaveCoverImageAsync(model.CoverImage);
            uploadedDigitalPath = await _fileStorageService.SaveDigitalBookAsync(model.DigitalFile);
        }
        catch (InvalidOperationException exception)
        {
            _fileStorageService.DeleteCoverIfOwned(uploadedCoverPath);
            _fileStorageService.DeleteDigitalBookIfOwned(uploadedDigitalPath);
            ModelState.AddModelError(string.Empty, exception.Message);
            model.BookId = id;
            model.ExistingCoverImagePath = book.CoverImagePath;
            model.ExistingDigitalFilePath = book.DigitalFilePath;
            await PopulateFormOptionsAsync(model);
            return View(model);
        }
        catch
        {
            _fileStorageService.DeleteCoverIfOwned(uploadedCoverPath);
            _fileStorageService.DeleteDigitalBookIfOwned(uploadedDigitalPath);
            throw;
        }

        var coverPath = uploadedCoverPath ?? book.CoverImagePath;
        var digitalPath = uploadedDigitalPath
            ?? (model.RemoveDigitalFile || !_fileStorageService.DigitalFileExists(book.DigitalFilePath) ? null : book.DigitalFilePath);

        book.Title = model.Title;
        book.ISBN = model.ISBN;
        book.Description = model.Description;
        book.AuthorId = model.AuthorId;
        book.CategoryId = model.CategoryId;
        book.PublisherId = model.PublisherId;
        book.PublishedYear = model.PublishedYear;
        book.Edition = model.Edition;
        book.Language = model.Language;
        book.TotalCopies = model.TotalCopies;
        book.AvailableCopies = Math.Max(0, model.TotalCopies - _context.BorrowRecords.Count(br => br.BookId == id && br.Status != BorrowStatus.Returned));
        book.ShelfLocation = model.ShelfLocation;
        book.CoverImagePath = coverPath;
        book.DigitalFilePath = digitalPath;
        book.IsDigitalAvailable = _fileStorageService.DigitalFileExists(digitalPath);
        book.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            _fileStorageService.DeleteCoverIfOwned(uploadedCoverPath);
            _fileStorageService.DeleteDigitalBookIfOwned(uploadedDigitalPath);
            throw;
        }

        if (uploadedCoverPath != null)
        {
            _fileStorageService.DeleteCoverIfOwned(previousCoverPath);
        }

        if (previousDigitalPath != digitalPath)
        {
            _fileStorageService.DeleteDigitalBookIfOwned(previousDigitalPath);
        }

        TempData["SuccessMessage"] = "Book updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Librarian")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book == null)
        {
            return NotFound();
        }

        if (await _context.BorrowRecords.AnyAsync(record => record.BookId == id)
            || await _context.Reservations.AnyAsync(reservation => reservation.BookId == id))
        {
            TempData["ErrorMessage"] = "This book cannot be deleted because it has borrowing or reservation history.";
            return RedirectToAction(nameof(Index));
        }

        _context.Books.Remove(book);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "This book could not be deleted because related library records still reference it.";
            return RedirectToAction(nameof(Index));
        }

        _fileStorageService.DeleteCoverIfOwned(book.CoverImagePath);
        _fileStorageService.DeleteDigitalBookIfOwned(book.DigitalFilePath);

        TempData["SuccessMessage"] = "Book deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Search(string query)
    {
        return RedirectToAction(nameof(Index), new { SearchTerm = query });
    }

    private async Task<BookFormViewModel> BuildBookFormViewModelAsync()
    {
        var model = new BookFormViewModel();
        await PopulateFormOptionsAsync(model);
        return model;
    }

    private async Task PopulateFormOptionsAsync(BookFormViewModel model)
    {
        model.Authors = await _context.Authors
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem { Value = a.AuthorId.ToString(), Text = a.Name })
            .ToListAsync();

        model.Categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
            .ToListAsync();

        model.Publishers = await _context.Publishers
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem { Value = p.PublisherId.ToString(), Text = p.Name })
            .ToListAsync();
    }
}
