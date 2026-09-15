using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class Exam
{
    [Key]
    public int ExamId { get; set; }

    [Required, MaxLength(200)]
    public string Course { get; set; } = string.Empty;

    [Required]
    public int Semester { get; set; }

    [Required]
    public DateTime ExamDate { get; set; }

    [Required, MaxLength(50)]
    public string TimeSlot { get; set; } = string.Empty;

    public ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
