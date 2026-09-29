using System.ComponentModel.DataAnnotations;

namespace DigitalLibraryManagementSystem.Models;

public class Publisher
{
    [Key]
    public int PublisherId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Address { get; set; }

    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }

    public ICollection<Book> Books { get; set; } = new HashSet<Book>();
}
