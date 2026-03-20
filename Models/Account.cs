using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public enum AccountType { Savings, Current, FixedDeposit }
    public enum AccountStatus { Active, Frozen, Closed }

    public class Account
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public AccountType AccountType { get; set; }

        public decimal Balance { get; set; } = 0;

        public AccountStatus Status { get; set; } = AccountStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Customer Customer { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<InstantCredit> InstantCredits { get; set; } = new List<InstantCredit>();
    }
}
