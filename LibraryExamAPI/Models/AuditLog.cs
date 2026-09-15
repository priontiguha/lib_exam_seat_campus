using System.ComponentModel.DataAnnotations;

namespace LibraryExamAPI.Models;

public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    [Required, MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [MaxLength(200)]
    public string? PerformedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? Details { get; set; }
}
