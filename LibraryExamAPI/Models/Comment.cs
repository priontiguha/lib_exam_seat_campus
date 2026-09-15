using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class Comment
{
    [Key]
    public int CommentId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required, MaxLength(2000)]
    public string CommentText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
