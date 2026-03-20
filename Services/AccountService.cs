using BankManagementSystem.Data.Repositories;
using BankManagementSystem.DTOs;
using BankManagementSystem.Models;

namespace BankManagementSystem.Services
{
    public interface IAccountService
    {
        Task<(bool Success, string Message)> RequestAccountAsync(int customerId, AccountRequestDto dto);
        Task<List<Account>> GetCustomerAccountsAsync(int customerId);
        Task<Account?> GetAccountByIdAsync(int accountId);
        Task<List<AccountRequest>> GetPendingRequestsAsync();
        Task<List<AccountRequest>> GetAllRequestsAsync();
        Task<List<AccountRequest>> GetRequestsByCustomerIdAsync(int customerId);
        Task<AccountRequest?> GetRequestByIdAsync(int id);
        Task<(bool Success, string Message)> ApproveRequestAsync(int requestId);
        Task<(bool Success, string Message)> RejectRequestAsync(int requestId, string notes);
        Task<(bool Success, string Message)> FreezeAccountAsync(int accountId);
        Task<(bool Success, string Message)> UnfreezeAccountAsync(int accountId);
        Task<(bool Success, string Message)> CloseAccountAsync(int accountId);
        Task<(bool Success, string Message)> ReopenAccountAsync(int accountId);
        Task<(bool Success, string Message)> DeleteAccountAsync(int accountId);
        Task<(bool Success, string Message)> AdminCreditAsync(int accountId, decimal amount, string note);
        Task<List<Account>> GetAllAccountsAsync();
    }

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accounts;
        private readonly IAccountRequestRepository _requests;
        private readonly ITransactionRepository _transactions;
        private readonly IAuditLogRepository _audit;

        public AccountService(IAccountRepository accounts, IAccountRequestRepository requests,
            ITransactionRepository transactions, IAuditLogRepository audit)
        {
            _accounts = accounts;
            _requests = requests;
            _transactions = transactions;
            _audit = audit;
        }

        public async Task<(bool Success, string Message)> RequestAccountAsync(int customerId, AccountRequestDto dto)
        {
            var request = new AccountRequest
            {
                CustomerId = customerId,
                AccountType = dto.AccountType,
                Address = dto.Address,
                Occupation = dto.Occupation,
                MonthlySalary = dto.MonthlySalary,
                FamilyMembers = dto.FamilyMembers,
                NomineeName = dto.NomineeName,
                NomineeRelation = dto.NomineeRelation,
                BusinessName = dto.BusinessName,
                BusinessType = dto.BusinessType,
                BusinessAddress = dto.BusinessAddress,
                AnnualIncome = dto.AnnualIncome,
                EmployeesCount = dto.EmployeesCount,
                DepositAmount = dto.DepositAmount,
                DurationMonths = dto.DurationMonths
            };
            await _requests.AddAsync(request);
            await _requests.SaveAsync();
            return (true, "Account request submitted successfully. Awaiting admin approval.");
        }

        public async Task<List<Account>> GetCustomerAccountsAsync(int customerId) =>
            await _accounts.GetByCustomerIdAsync(customerId);

        public async Task<Account?> GetAccountByIdAsync(int accountId) =>
            await _accounts.GetByIdAsync(accountId);

        public async Task<List<AccountRequest>> GetPendingRequestsAsync() =>
            await _requests.GetPendingAsync();

        public async Task<List<AccountRequest>> GetAllRequestsAsync() =>
            await _requests.GetAllAsync();

        public async Task<List<AccountRequest>> GetRequestsByCustomerIdAsync(int customerId) =>
            await _requests.GetByCustomerIdAsync(customerId);

        public async Task<AccountRequest?> GetRequestByIdAsync(int id) =>
            await _requests.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> ApproveRequestAsync(int requestId)
        {
            var request = await _requests.GetByIdAsync(requestId);
            if (request == null) return (false, "Request not found.");
            if (request.Status != RequestStatus.Pending) return (false, "Request already processed.");

            var accountNumber = GenerateAccountNumber();
            var account = new Account
            {
                AccountNumber = accountNumber,
                CustomerId = request.CustomerId,
                AccountType = request.AccountType,
                Balance = 10000,
                Status = AccountStatus.Active
            };

            await _accounts.AddAsync(account);
            request.Status = RequestStatus.Approved;
            await _requests.UpdateAsync(request);
            await _requests.SaveAsync();

            // Log the opening deposit
            await _transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Deposit,
                Amount = 10000,
                Description = "Account opening — minimum balance seeded by NEXBank"
            });
            await _transactions.SaveAsync();

            await _audit.AddAsync(new AuditLog { Action = $"Account {accountNumber} approved for customer {request.CustomerId}" });
            await _audit.SaveAsync();

            return (true, $"Account approved. Account number: {accountNumber}");
        }

        public async Task<(bool Success, string Message)> RejectRequestAsync(int requestId, string notes)
        {
            var request = await _requests.GetByIdAsync(requestId);
            if (request == null) return (false, "Request not found.");
            if (request.Status != RequestStatus.Pending) return (false, "Request already processed.");

            request.Status = RequestStatus.Rejected;
            request.AdminNotes = string.IsNullOrWhiteSpace(notes) ? "No reason provided." : notes;
            await _requests.UpdateAsync(request);
            await _requests.SaveAsync();
            return (true, "Request rejected.");
        }

        public async Task<(bool Success, string Message)> FreezeAccountAsync(int accountId)
        {
            var account = await _accounts.GetByIdAsync(accountId);
            if (account == null) return (false, "Account not found.");
            if (account.Status == AccountStatus.Frozen) return (false, "Account is already frozen.");
            account.Status = AccountStatus.Frozen;
            await _accounts.UpdateAsync(account);
            await _accounts.SaveAsync();
            await _audit.AddAsync(new AuditLog { Action = $"Account {account.AccountNumber} frozen" });
            await _audit.SaveAsync();
            return (true, "Account frozen.");
        }

        public async Task<(bool Success, string Message)> UnfreezeAccountAsync(int accountId)
        {
            var account = await _accounts.GetByIdAsync(accountId);
            if (account == null) return (false, "Account not found.");
            account.Status = AccountStatus.Active;
            await _accounts.UpdateAsync(account);
            await _accounts.SaveAsync();
            await _audit.AddAsync(new AuditLog { Action = $"Account {account.AccountNumber} unfrozen" });
            await _audit.SaveAsync();
            return (true, "Account unfrozen.");
        }

        public async Task<(bool Success, string Message)> CloseAccountAsync(int accountId)
        {
            var account = await _accounts.GetByIdAsync(accountId);
            if (account == null) return (false, "Account not found.");
            account.Status = AccountStatus.Closed;
            await _accounts.UpdateAsync(account);
            await _accounts.SaveAsync();
            await _audit.AddAsync(new AuditLog { Action = $"Account {account.AccountNumber} closed" });
            await _audit.SaveAsync();
            return (true, "Account closed.");
        }

        public async Task<(bool Success, string Message)> ReopenAccountAsync(int accountId)
        {
            var account = await _accounts.GetByIdAsync(accountId);
            if (account == null) return (false, "Account not found.");
            if (account.Status != AccountStatus.Closed) return (false, "Account is not closed.");
            account.Status = AccountStatus.Active;
            await _accounts.UpdateAsync(account);
            await _accounts.SaveAsync();
            await _audit.AddAsync(new AuditLog { Action = $"Account {account.AccountNumber} reopened" });
            await _audit.SaveAsync();
            return (true, "Account reopened successfully.");
        }

        public async Task<(bool Success, string Message)> AdminCreditAsync(int accountId, decimal amount, string note)
        {
            if (amount <= 0) return (false, "Amount must be greater than zero.");
            var account = await _accounts.GetByIdAsync(accountId);
            if (account == null) return (false, "Account not found.");
            if (account.Status == AccountStatus.Closed) return (false, "Cannot credit a closed account.");

            account.Balance += amount;
            await _accounts.UpdateAsync(account);

            await _transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Deposit,
                Amount = amount,
                Description = $"Admin credit: {(string.IsNullOrWhiteSpace(note) ? "Manual adjustment" : note)}"
            });
            await _transactions.SaveAsync();

            await _audit.AddAsync(new AuditLog { Action = $"Admin credited ₹{amount} to account {account.AccountNumber}. Note: {note}" });
            await _audit.SaveAsync();

            return (true, $"₹{amount:N2} credited to account {account.AccountNumber}.");
        }

        public async Task<(bool Success, string Message)> DeleteAccountAsync(int accountId)
        {
            var account = await _accounts.GetByIdAsync(accountId);
            if (account == null) return (false, "Account not found.");
            await _accounts.DeleteAsync(account);
            await _accounts.SaveAsync();
            return (true, "Account deleted.");
        }

        public async Task<List<Account>> GetAllAccountsAsync() =>
            await _accounts.GetAllAsync();

        private static string GenerateAccountNumber()
        {
            var random = new Random();
            return "NEX" + DateTime.UtcNow.Ticks.ToString()[^6..] + random.Next(100, 999);
        }
    }
}
