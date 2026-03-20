using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [MaxLength(200)]
        public string UserEmail { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? IpAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
