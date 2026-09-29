using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibraryManagementSystem.Models;

public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int BookId { get; set; }

    public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [ForeignKey(nameof(BookId))]
    public Book? Book { get; set; }
}
