using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class Invigilator
{
    [Key]
    public int StaffId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Dept { get; set; } = string.Empty;

    public ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
