using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibraryManagementSystem.Models;

public class BorrowRecord
{
    [Key]
    public int BorrowRecordId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int BookId { get; set; }

    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public BorrowStatus Status { get; set; } = BorrowStatus.Borrowed;

    [Column(TypeName = "decimal(10,2)")]
    public decimal FineAmount { get; set; }

    public bool IsFinePaid { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [ForeignKey(nameof(BookId))]
    public Book? Book { get; set; }

    public ICollection<Fine> Fines { get; set; } = new HashSet<Fine>();
}
