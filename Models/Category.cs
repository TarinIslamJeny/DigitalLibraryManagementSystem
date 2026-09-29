using System.ComponentModel.DataAnnotations;

namespace DigitalLibraryManagementSystem.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new HashSet<Book>();
}
