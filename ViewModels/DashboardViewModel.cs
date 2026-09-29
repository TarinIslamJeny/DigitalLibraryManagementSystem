namespace DigitalLibraryManagementSystem.ViewModels;

public class DashboardViewModel
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int BorrowedBooks { get; set; }
    public int DigitalBooks { get; set; }
    public int TotalMembers { get; set; }
    public int Librarians { get; set; }
    public int Reservations { get; set; }
    public int OverdueBooks { get; set; }
    public decimal UnpaidFines { get; set; }
}
