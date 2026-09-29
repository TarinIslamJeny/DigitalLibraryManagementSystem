using DigitalLibraryManagementSystem.Data;
using DigitalLibraryManagementSystem.Models;
using DigitalLibraryManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardSummaryAsync()
    {
        var today = DateTime.UtcNow;
        var viewModel = new DashboardViewModel
        {
            TotalBooks = await _context.Books.CountAsync(),
            AvailableBooks = await _context.Books.Where(b => b.AvailableCopies > 0).CountAsync(),
            BorrowedBooks = await _context.BorrowRecords.CountAsync(br => br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue),
            TotalMembers = await _context.Users.CountAsync(),
            Librarians = await _context.UserRoles
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .CountAsync(x => x.Name == "Librarian"),
            Reservations = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Available),
            OverdueBooks = await _context.BorrowRecords.CountAsync(br => br.Status == BorrowStatus.Overdue || (br.Status == BorrowStatus.Borrowed && br.DueDate < today)),
            UnpaidFines = await _context.Fines.Where(f => f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0m
        };

        return viewModel;
    }
}
