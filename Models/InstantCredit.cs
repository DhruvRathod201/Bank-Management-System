using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public enum CreditStatus { PendingSettlement, Settled, Overdue }

    public class InstantCredit
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public CreditStatus Status { get; set; } = CreditStatus.PendingSettlement;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(3);

        public DateTime? SettledAt { get; set; }

        // Navigation
        public Account Account { get; set; } = null!;
    }
}
