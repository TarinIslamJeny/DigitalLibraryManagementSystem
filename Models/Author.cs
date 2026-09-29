using System.ComponentModel.DataAnnotations;

namespace DigitalLibraryManagementSystem.Models;

public class Author
{
    [Key]
    public int AuthorId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Biography { get; set; }

    public ICollection<Book> Books { get; set; } = new HashSet<Book>();
}
