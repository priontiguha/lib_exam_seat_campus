using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryExamAPI.Models;

public class IssueRecord
{
    [Key]
    public int IssueId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public DateTime IssueDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal FineAmount { get; set; }

    public Book Book { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
