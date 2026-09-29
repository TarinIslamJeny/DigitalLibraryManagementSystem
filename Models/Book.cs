using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibraryManagementSystem.Models;

public class Book
{
    [Key]
    public int BookId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ISBN { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [Range(0, 9999)]
    public int PublishedYear { get; set; }

    [StringLength(50)]
    public string? Edition { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableCopies { get; set; }

    [StringLength(100)]
    public string? ShelfLocation { get; set; }

    [StringLength(255)]
    public string? CoverImagePath { get; set; }

    [StringLength(255)]
    public string? DigitalFilePath { get; set; }

    public bool IsDigitalAvailable { get; set; }

    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public int PublisherId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public Author? Author { get; set; }

    [ForeignKey(nameof(PublisherId))]
    public Publisher? Publisher { get; set; }

    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new HashSet<BorrowRecord>();
    public ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
    public ICollection<BookReview> Reviews { get; set; } = new HashSet<BookReview>();
}
