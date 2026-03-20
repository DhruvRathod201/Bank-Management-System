using BankManagementSystem.Data.Repositories;
using BankManagementSystem.DTOs;
using BankManagementSystem.Models;

namespace BankManagementSystem.Services
{
    public interface ITransactionService
    {
        Task<(bool Success, string Message)> DepositAsync(int customerId, DepositDto dto);
        Task<(bool Success, string Message)> WithdrawAsync(int customerId, WithdrawDto dto);
        Task<(bool Success, string Message)> TransferAsync(int customerId, TransferDto dto);
        Task<List<Transaction>> GetAccountTransactionsAsync(int accountId);
        Task<List<Transaction>> GetFilteredTransactionsAsync(TransactionFilterDto filter);
        Task<List<Transaction>> GetAllTransactionsAsync();
    }

    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accounts;
        private readonly ITransactionRepository _transactions;
        private readonly IInstantCreditRepository _credits;
        private readonly IAuthService _auth;
        private readonly IAuditLogRepository _audit;

        public const decimal MinBalance = 10000m;
        public const decimal MaxInstantCredit = 10000m;

        public TransactionService(IAccountRepository accounts, ITransactionRepository transactions,
            IInstantCreditRepository credits, IAuthService auth, IAuditLogRepository audit)
        {
            _accounts = accounts;
            _transactions = transactions;
            _credits = credits;
            _auth = auth;
            _audit = audit;
        }

        public async Task<(bool Success, string Message)> DepositAsync(int customerId, DepositDto dto)
        {
            var account = await _accounts.GetByIdAsync(dto.AccountId);
            if (account == null) return (false, "Account not found.");
            if (account.CustomerId != customerId) return (false, "Unauthorized.");

            // FIX: Frozen accounts cannot use instant credit — only admin deposit works
            if (account.Status == AccountStatus.Frozen)
                return (false, "Account is frozen. Only an admin can deposit to this account.");
            if (account.Status == AccountStatus.Closed)
                return (false, "Account is closed.");

            if (dto.Amount <= 0 || dto.Amount != Math.Floor(dto.Amount))
                return (false, "Amount must be a whole positive number.");
            if (dto.Amount > MaxInstantCredit)
                return (false, $"Maximum instant credit is ₹{MaxInstantCredit:N0}.");

            var pendingCredits = await _credits.GetByAccountIdAsync(account.Id);
            if (pendingCredits.Any(c => c.Status == CreditStatus.PendingSettlement))
                return (false, "You have an unsettled instant credit. Please settle it before requesting another.");

            account.Balance += dto.Amount;
            await _accounts.UpdateAsync(account);

            await _transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.InstantCredit,
                Amount = dto.Amount,
                Description = "Instant Credit deposited. Settlement due in 3 days."
            });
            await _transactions.SaveAsync();

            await _credits.AddAsync(new InstantCredit
            {
                AccountId = account.Id,
                Amount = dto.Amount,
                Status = CreditStatus.PendingSettlement,
                Deadline = DateTime.UtcNow.AddDays(3)
            });
            await _credits.SaveAsync();

            await _audit.AddAsync(new AuditLog { Action = $"Instant credit ₹{dto.Amount} on account {account.AccountNumber}" });
            await _audit.SaveAsync();

            return (true, $"₹{dto.Amount:N0} credited. Please repay at the bank branch within 3 days.");
        }

        public async Task<(bool Success, string Message)> WithdrawAsync(int customerId, WithdrawDto dto)
        {
            var account = await _accounts.GetByIdAsync(dto.AccountId);
            if (account == null) return (false, "Account not found.");
            if (account.CustomerId != customerId) return (false, "Unauthorized.");
            if (account.Status == AccountStatus.Frozen) return (false, "Account is frozen. You cannot withdraw.");
            if (account.Status == AccountStatus.Closed) return (false, "Account is closed.");

            if (dto.Amount <= 0 || dto.Amount != Math.Floor(dto.Amount))
                return (false, "Amount must be a whole positive number.");

            if (account.Balance - dto.Amount < MinBalance)
                return (false, $"Insufficient balance. You can withdraw at most ₹{Math.Max(0, account.Balance - MinBalance):N0}. A minimum balance of ₹{MinBalance:N0} must always be maintained.");

            account.Balance -= dto.Amount;
            await _accounts.UpdateAsync(account);

            await _transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Withdraw,
                Amount = dto.Amount,
                Description = "Withdrawal"
            });
            await _transactions.SaveAsync();

            await _audit.AddAsync(new AuditLog { Action = $"Withdrawal ₹{dto.Amount} from account {account.AccountNumber}" });
            await _audit.SaveAsync();

            return (true, $"₹{dto.Amount:N0} withdrawn successfully.");
        }

        public async Task<(bool Success, string Message)> TransferAsync(int customerId, TransferDto dto)
        {
            var fromAccount = await _accounts.GetByIdAsync(dto.FromAccountId);
            if (fromAccount == null) return (false, "Source account not found.");
            if (fromAccount.CustomerId != customerId) return (false, "Unauthorized.");
            if (fromAccount.Status == AccountStatus.Frozen) return (false, "Source account is frozen.");
            if (fromAccount.Status == AccountStatus.Closed) return (false, "Source account is closed.");

            if (string.IsNullOrWhiteSpace(dto.ReceiverAccountNumber))
                return (false, "Receiver account number is required.");

            var toAccount = await _accounts.GetByNumberAsync(dto.ReceiverAccountNumber.Trim());
            if (toAccount == null)
                return (false, $"Receiver account '{dto.ReceiverAccountNumber}' does not exist. Please check the account number.");
            if (toAccount.Id == fromAccount.Id)
                return (false, "Cannot transfer to the same account.");
            if (toAccount.Status == AccountStatus.Closed)
                return (false, "Receiver account is closed. Transfer not allowed.");
            if (toAccount.Status == AccountStatus.Frozen)
                return (false, "Receiver account is frozen. Transfer not allowed.");

            if (dto.Amount <= 0 || dto.Amount != Math.Floor(dto.Amount))
                return (false, "Amount must be a whole positive number.");

            if (fromAccount.Balance - dto.Amount < MinBalance)
                return (false, $"Insufficient balance. You can transfer at most ₹{Math.Max(0, fromAccount.Balance - MinBalance):N0}. A minimum balance of ₹{MinBalance:N0} must always be maintained.");

            fromAccount.Balance -= dto.Amount;
            toAccount.Balance += dto.Amount;
            await _accounts.UpdateAsync(fromAccount);
            await _accounts.UpdateAsync(toAccount);

            await _transactions.AddAsync(new Transaction
            {
                AccountId = fromAccount.Id,
                Type = TransactionType.Transfer,
                Amount = dto.Amount,
                Description = $"Transfer to {dto.ReceiverAccountNumber}",
                RelatedAccountNumber = dto.ReceiverAccountNumber
            });
            await _transactions.AddAsync(new Transaction
            {
                AccountId = toAccount.Id,
                Type = TransactionType.Transfer,
                Amount = dto.Amount,
                Description = $"Transfer from {fromAccount.AccountNumber}",
                RelatedAccountNumber = fromAccount.AccountNumber
            });
            await _transactions.SaveAsync();

            await _audit.AddAsync(new AuditLog { Action = $"Transfer ₹{dto.Amount} from {fromAccount.AccountNumber} to {toAccount.AccountNumber}" });
            await _audit.SaveAsync();

            return (true, $"₹{dto.Amount:N0} transferred to {dto.ReceiverAccountNumber} successfully.");
        }

        public async Task<List<Transaction>> GetAccountTransactionsAsync(int accountId) =>
            await _transactions.GetByAccountIdAsync(accountId);

        public async Task<List<Transaction>> GetFilteredTransactionsAsync(TransactionFilterDto filter) =>
            await _transactions.GetFilteredAsync(filter.AccountId, filter.Type, filter.FromDate, filter.ToDate);

        public async Task<List<Transaction>> GetAllTransactionsAsync() =>
            await _transactions.GetAllAsync();
    }
}
