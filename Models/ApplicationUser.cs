using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DigitalLibraryManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(255)]
    public string? ProfilePicture { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new HashSet<BorrowRecord>();
    public ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
    public ICollection<BookReview> Reviews { get; set; } = new HashSet<BookReview>();
    public ICollection<Fine> Fines { get; set; } = new HashSet<Fine>();
    public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
}
