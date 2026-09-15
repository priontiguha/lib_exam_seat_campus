using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class Reaction
{
    [Key]
    public int ReactionId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required, MaxLength(20)]
    public string Type { get; set; } = "like";

    public Book Book { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
