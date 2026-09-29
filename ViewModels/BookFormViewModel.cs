using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DigitalLibraryManagementSystem.ViewModels;

public class BookFormViewModel
{
    public int? BookId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ISBN { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Author")]
    public int AuthorId { get; set; }

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "Publisher")]
    public int PublisherId { get; set; }

    [Display(Name = "Published Year")]
    [Range(0, 9999)]
    public int PublishedYear { get; set; }

    [StringLength(50)]
    public string? Edition { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [Display(Name = "Total Copies")]
    [Range(0, int.MaxValue)]
    public int TotalCopies { get; set; }

    [Display(Name = "Shelf Location")]
    [StringLength(100)]
    public string? ShelfLocation { get; set; }

    public string? ExistingCoverImagePath { get; set; }
    public string? ExistingDigitalFilePath { get; set; }

    [Display(Name = "Remove current digital PDF")]
    public bool RemoveDigitalFile { get; set; }

    [Display(Name = "Cover Image")]
    public IFormFile? CoverImage { get; set; }

    [Display(Name = "Digital PDF")]
    public IFormFile? DigitalFile { get; set; }

    public IEnumerable<SelectListItem>? Authors { get; set; }
    public IEnumerable<SelectListItem>? Categories { get; set; }
    public IEnumerable<SelectListItem>? Publishers { get; set; }
}
