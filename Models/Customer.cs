using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public enum CustomerStatus { Pending, Active, Blocked, Rejected }

    public class Customer
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public CustomerStatus Status { get; set; } = CustomerStatus.Pending;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<AccountRequest> AccountRequests { get; set; } = new List<AccountRequest>();
    }
}
