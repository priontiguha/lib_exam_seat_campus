using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required, MaxLength(50)]
    public string RoomNo { get; set; } = string.Empty;

    [Required]
    public int Capacity { get; set; }

    [Required, MaxLength(200)]
    public string BenchLayout { get; set; } = string.Empty;

    public ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
