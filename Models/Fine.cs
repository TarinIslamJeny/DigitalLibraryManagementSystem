using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibraryManagementSystem.Models;

public class Fine
{
    [Key]
    public int FineId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int BorrowRecordId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [StringLength(250)]
    public string? Reason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaidDate { get; set; }

    public FineStatus Status { get; set; } = FineStatus.Unpaid;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [ForeignKey(nameof(BorrowRecordId))]
    public BorrowRecord? BorrowRecord { get; set; }
}
