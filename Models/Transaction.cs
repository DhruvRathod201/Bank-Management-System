using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public enum TransactionType { Deposit, Withdraw, Transfer, InstantCredit, CreditSettlement }

    public class Transaction
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // For transfers: store related account
        public string? RelatedAccountNumber { get; set; }

        // Navigation
        public Account Account { get; set; } = null!;
    }
}
