using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class Book
{
    [Key]
    public int BookId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Isbn { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Genre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? CoverImageUrl { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Publisher { get; set; }

    public int PublishedYear { get; set; } = DateTime.UtcNow.Year;

    public int CopiesAvailable { get; set; }

    public ICollection<IssueRecord> IssueRecords { get; set; } = new List<IssueRecord>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}
