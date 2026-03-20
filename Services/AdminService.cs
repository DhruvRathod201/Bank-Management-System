using BankManagementSystem.Data.Repositories;
using BankManagementSystem.DTOs;
using BankManagementSystem.Models;

namespace BankManagementSystem.Services
{
    public interface ICustomerService
    {
        Task<Customer?> GetByUserIdAsync(int userId);
        Task<Customer?> GetByIdAsync(int id);
        Task<List<Customer>> GetAllAsync();
        Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customers;
        private readonly IUserRepository _users;

        public CustomerService(ICustomerRepository customers, IUserRepository users)
        {
            _customers = customers;
            _users = users;
        }

        public async Task<Customer?> GetByUserIdAsync(int userId) => await _customers.GetByUserIdAsync(userId);
        public async Task<Customer?> GetByIdAsync(int id) => await _customers.GetByIdAsync(id);
        public async Task<List<Customer>> GetAllAsync() => await _customers.GetAllAsync();

        public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _users.GetByIdAsync(userId);
            var customer = await _customers.GetByUserIdAsync(userId);
            if (user == null || customer == null) return (false, "User not found.");

            customer.Phone = dto.Phone;
            customer.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                    return (false, "Current password is required to change password.");
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                    return (false, "Current password is incorrect.");
                if (dto.NewPassword != dto.ConfirmNewPassword)
                    return (false, "New passwords do not match.");
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _users.UpdateAsync(user);
            }

            await _customers.UpdateAsync(customer);
            await _customers.SaveAsync();
            await _users.SaveAsync();
            return (true, "Profile updated successfully.");
        }
    }

    public interface IAdminService
    {
        Task<(bool Success, string Message)> ApproveCustomerAsync(int customerId);
        Task<(bool Success, string Message)> RejectCustomerAsync(int customerId);
        Task<(bool Success, string Message)> BlockCustomerAsync(int customerId);
        Task<(bool Success, string Message)> UnblockCustomerAsync(int customerId);
        Task<(bool Success, string Message)> DeleteCustomerAsync(int customerId);
        Task<(int Customers, int Accounts, int Transactions, int PendingRequests)> GetDashboardStatsAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly ICustomerRepository _customers;
        private readonly IUserRepository _users;
        private readonly IAccountRepository _accounts;
        private readonly ITransactionRepository _transactions;
        private readonly IAccountRequestRepository _requests;
        private readonly IAuditLogRepository _audit;

        public AdminService(ICustomerRepository customers, IUserRepository users, IAccountRepository accounts,
            ITransactionRepository transactions, IAccountRequestRepository requests, IAuditLogRepository audit)
        {
            _customers = customers;
            _users = users;
            _accounts = accounts;
            _transactions = transactions;
            _requests = requests;
            _audit = audit;
        }

        public async Task<(bool Success, string Message)> ApproveCustomerAsync(int customerId)
        {
            var customer = await _customers.GetByIdAsync(customerId);
            if (customer == null) return (false, "Customer not found.");
            customer.Status = CustomerStatus.Active;
            await _customers.UpdateAsync(customer);
            await _customers.SaveAsync();
            await _audit.AddAsync(new AuditLog { Action = $"Customer {customer.User.Email} approved" });
            await _audit.SaveAsync();
            return (true, "Customer approved.");
        }

        public async Task<(bool Success, string Message)> RejectCustomerAsync(int customerId)
        {
            var customer = await _customers.GetByIdAsync(customerId);
            if (customer == null) return (false, "Customer not found.");
            customer.Status = CustomerStatus.Rejected;
            await _customers.UpdateAsync(customer);
            await _customers.SaveAsync();
            return (true, "Customer rejected.");
        }

        public async Task<(bool Success, string Message)> BlockCustomerAsync(int customerId)
        {
            var customer = await _customers.GetByIdAsync(customerId);
            if (customer == null) return (false, "Customer not found.");
            customer.Status = CustomerStatus.Blocked;
            // Also deactivate the user login
            var user = await _users.GetByIdAsync(customer.UserId);
            if (user != null) { user.IsActive = false; await _users.UpdateAsync(user); }
            await _customers.UpdateAsync(customer);
            await _customers.SaveAsync();
            await _users.SaveAsync();
            await _audit.AddAsync(new AuditLog { Action = $"Customer {customer.User.Email} blocked" });
            await _audit.SaveAsync();
            return (true, "Customer blocked. They can no longer log in.");
        }

        public async Task<(bool Success, string Message)> UnblockCustomerAsync(int customerId)
        {
            var customer = await _customers.GetByIdAsync(customerId);
            if (customer == null) return (false, "Customer not found.");
            customer.Status = CustomerStatus.Active;
            var user = await _users.GetByIdAsync(customer.UserId);
            if (user != null) { user.IsActive = true; await _users.UpdateAsync(user); }
            await _customers.UpdateAsync(customer);
            await _customers.SaveAsync();
            await _users.SaveAsync();
            await _audit.AddAsync(new AuditLog { Action = $"Customer {customer.User.Email} unblocked" });
            await _audit.SaveAsync();
            return (true, "Customer unblocked.");
        }

        public async Task<(bool Success, string Message)> DeleteCustomerAsync(int customerId)
        {
            var customer = await _customers.GetByIdAsync(customerId);
            if (customer == null) return (false, "Customer not found.");
            var user = await _users.GetByIdAsync(customer.UserId);
            if (user != null) await _users.DeleteAsync(user);
            await _users.SaveAsync();
            return (true, "Customer deleted.");
        }

        public async Task<(int Customers, int Accounts, int Transactions, int PendingRequests)> GetDashboardStatsAsync()
        {
            var customers = (await _customers.GetAllAsync()).Count;
            var accounts = (await _accounts.GetAllAsync()).Count;
            var transactions = (await _transactions.GetAllAsync()).Count;
            var pending = (await _requests.GetPendingAsync()).Count;
            return (customers, accounts, transactions, pending);
        }
    }

    public interface IInstantCreditService
    {
        Task<(bool Success, string Message)> SettleAsync(int creditId);
        Task<(bool Success, string Message)> ExtendDeadlineAsync(int creditId, int days);
        Task<List<InstantCredit>> GetAllPendingAsync();
        Task ProcessOverdueCreditsAsync();
    }

    public class InstantCreditService : IInstantCreditService
    {
        private readonly IInstantCreditRepository _credits;
        private readonly IAccountRepository _accounts;
        private readonly ITransactionRepository _transactions;
        private readonly IAuditLogRepository _audit;
        private const decimal MinBalance = 10000m;

        public InstantCreditService(IInstantCreditRepository credits, IAccountRepository accounts,
            ITransactionRepository transactions, IAuditLogRepository audit)
        {
            _credits = credits;
            _accounts = accounts;
            _transactions = transactions;
            _audit = audit;
        }

        public async Task<(bool Success, string Message)> SettleAsync(int creditId)
        {
            var credit = await _credits.GetByIdAsync(creditId);
            if (credit == null) return (false, "Credit not found.");
            if (credit.Status != CreditStatus.PendingSettlement) return (false, "Credit is not pending settlement.");

            var account = await _accounts.GetByIdAsync(credit.AccountId);
            if (account == null) return (false, "Associated account not found.");

            // FIX: The settlement deducts the credit amount. But we must ensure
            // after deduction the balance doesn't go below MinBalance.
            // If the customer transferred out money, they may not have enough.
            // We still settle but note the situation — admin is explicitly settling.
            var balanceAfter = account.Balance - credit.Amount;
            if (balanceAfter < 0)
            {
                // Allow settlement but set balance to 0 (can't go negative)
                balanceAfter = 0;
            }

            credit.Status = CreditStatus.Settled;
            credit.SettledAt = DateTime.UtcNow;
            await _credits.UpdateAsync(credit);

            account.Balance = balanceAfter;
            // If balance is below minimum after settlement, unfreeze if frozen (admin made good)
            if (account.Status == AccountStatus.Frozen && balanceAfter >= MinBalance)
                account.Status = AccountStatus.Active;

            await _accounts.UpdateAsync(account);

            await _transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.CreditSettlement,
                Amount = credit.Amount,
                Description = $"Instant credit ₹{credit.Amount:N0} settled by admin. Balance after: ₹{balanceAfter:N0}"
            });
            await _transactions.SaveAsync();

            await _audit.AddAsync(new AuditLog { Action = $"Instant credit {creditId} settled. Account {account.AccountNumber} balance: ₹{balanceAfter:N0}" });
            await _audit.SaveAsync();

            return (true, $"Credit settled. Account {account.AccountNumber} balance is now ₹{balanceAfter:N0}.");
        }

        public async Task<(bool Success, string Message)> ExtendDeadlineAsync(int creditId, int days)
        {
            var credit = await _credits.GetByIdAsync(creditId);
            if (credit == null) return (false, "Credit not found.");
            if (days <= 0 || days > 30) return (false, "Extension must be between 1 and 30 days.");
            credit.Deadline = credit.Deadline.AddDays(days);
            await _credits.UpdateAsync(credit);
            await _credits.SaveAsync();
            return (true, $"Deadline extended by {days} days.");
        }

        public async Task<List<InstantCredit>> GetAllPendingAsync() => await _credits.GetAllPendingAsync();

        public async Task ProcessOverdueCreditsAsync()
        {
            var overdue = await _credits.GetOverdueAsync();
            foreach (var credit in overdue)
            {
                credit.Status = CreditStatus.Overdue;
                await _credits.UpdateAsync(credit);

                var account = await _accounts.GetByIdAsync(credit.AccountId);
                if (account != null)
                {
                    var deductible = Math.Min(credit.Amount, account.Balance);
                    account.Balance -= deductible;
                    if (account.Balance < 0) account.Balance = 0;
                    account.Status = AccountStatus.Frozen;
                    await _accounts.UpdateAsync(account);
                    await _transactions.AddAsync(new Transaction
                    {
                        AccountId = account.Id,
                        Type = TransactionType.CreditSettlement,
                        Amount = deductible,
                        Description = $"OVERDUE: Instant credit ₹{credit.Amount:N0} auto-deducted. Account frozen."
                    });
                }
            }
            if (overdue.Any()) await _credits.SaveAsync();
        }
    }
}
