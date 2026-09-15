using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class SeatAllocation
{
    [Key]
    public int SeatId { get; set; }

    [Required]
    public int ExamId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public int BenchNo { get; set; }
    public int SeatNo { get; set; }

    public int? InvigilatorId { get; set; }

    public Exam Exam { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Invigilator? Invigilator { get; set; }
}
