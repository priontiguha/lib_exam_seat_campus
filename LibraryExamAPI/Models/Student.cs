using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class Student
{
    [Key]
    public int StudentId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string RollNo { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Dept { get; set; } = string.Empty;

    [Required]
    public int Semester { get; set; }

    [Required, MaxLength(100)]
    public string Contact { get; set; } = string.Empty;

    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Role { get; set; } = "Student";

    public bool IsVerified { get; set; }

    public ICollection<IssueRecord> IssueRecords { get; set; } = new List<IssueRecord>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
