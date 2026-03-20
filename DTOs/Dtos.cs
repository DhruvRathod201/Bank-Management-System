using System.ComponentModel.DataAnnotations;
using BankManagementSystem.Models;

namespace BankManagementSystem.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }

    public class AccountRequestDto
    {
        [Required]
        public AccountType AccountType { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public decimal? MonthlySalary { get; set; }
        public int? FamilyMembers { get; set; }
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? BusinessAddress { get; set; }
        public decimal? AnnualIncome { get; set; }
        public int? EmployeesCount { get; set; }
        public decimal? DepositAmount { get; set; }
        public int? DurationMonths { get; set; }
    }

    public class DepositDto
    {
        [Required]
        public int AccountId { get; set; }

        // Step validation (whole numbers only 1–10000)
        [Required, Range(1, 10000, ErrorMessage = "Amount must be between ₹1 and ₹10,000.")]
        public decimal Amount { get; set; }
    }

    public class WithdrawDto
    {
        [Required]
        public int AccountId { get; set; }

        [Required, Range(1, double.MaxValue, ErrorMessage = "Amount must be at least ₹1.")]
        public decimal Amount { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class TransferDto
    {
        [Required]
        public int FromAccountId { get; set; }

        [Required]
        public string ReceiverAccountNumber { get; set; } = string.Empty;

        [Required, Range(1, double.MaxValue, ErrorMessage = "Amount must be at least ₹1.")]
        public decimal Amount { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class TransactionFilterDto
    {
        public int? AccountId { get; set; }
        public TransactionType? Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
