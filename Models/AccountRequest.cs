using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public enum RequestStatus { Pending, Approved, Rejected }

    public class AccountRequest
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public AccountType AccountType { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Savings fields
        public string? Occupation { get; set; }
        public decimal? MonthlySalary { get; set; }
        public int? FamilyMembers { get; set; }
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }

        // Current account fields
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? BusinessAddress { get; set; }
        public decimal? AnnualIncome { get; set; }
        public int? EmployeesCount { get; set; }

        // FD fields
        public decimal? DepositAmount { get; set; }
        public int? DurationMonths { get; set; }

        // Shared address (for savings)
        public string? Address { get; set; }

        public string? AdminNotes { get; set; }

        // Navigation
        public Customer Customer { get; set; } = null!;
    }
}
