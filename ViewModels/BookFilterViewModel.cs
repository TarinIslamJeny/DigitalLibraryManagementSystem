namespace DigitalLibraryManagementSystem.ViewModels;

public class BookFilterViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int? AuthorId { get; set; }
    public string Availability { get; set; } = "All";
    public string DigitalAvailability { get; set; } = "All";
    public int? PublishedYear { get; set; }
    public string SortBy { get; set; } = "TitleAsc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 8;
}
