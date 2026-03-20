using BankManagementSystem.Data;
using BankManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BankManagementSystem.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task SaveAsync();
    }

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) => _db = db;

        public async Task<User?> GetByEmailAsync(string email) =>
            await _db.Users.Include(u => u.Customer).FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByIdAsync(int id) =>
            await _db.Users.Include(u => u.Customer).FirstOrDefaultAsync(u => u.Id == id);

        public async Task<List<User>> GetAllAsync() =>
            await _db.Users.Include(u => u.Customer).ToListAsync();

        public async Task AddAsync(User user) => await _db.Users.AddAsync(user);
        public Task UpdateAsync(User user) { _db.Users.Update(user); return Task.CompletedTask; }
        public Task DeleteAsync(User user) { _db.Users.Remove(user); return Task.CompletedTask; }
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }

    public interface ICustomerRepository
    {
        Task<Customer?> GetByUserIdAsync(int userId);
        Task<Customer?> GetByIdAsync(int id);
        Task<List<Customer>> GetAllAsync();
        Task<List<Customer>> GetPendingAsync();
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(Customer customer);
        Task SaveAsync();
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _db;
        public CustomerRepository(ApplicationDbContext db) => _db = db;

        public async Task<Customer?> GetByUserIdAsync(int userId) =>
            await _db.Customers.Include(c => c.User).Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task<Customer?> GetByIdAsync(int id) =>
            await _db.Customers.Include(c => c.User).Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<List<Customer>> GetAllAsync() =>
            await _db.Customers.Include(c => c.User).Include(c => c.Accounts).ToListAsync();

        public async Task<List<Customer>> GetPendingAsync() =>
            await _db.Customers.Include(c => c.User)
                .Where(c => c.Status == CustomerStatus.Pending).ToListAsync();

        public async Task AddAsync(Customer customer) => await _db.Customers.AddAsync(customer);
        public Task UpdateAsync(Customer customer) { _db.Customers.Update(customer); return Task.CompletedTask; }
        public Task DeleteAsync(Customer customer) { _db.Customers.Remove(customer); return Task.CompletedTask; }
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }

    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByNumberAsync(string accountNumber);
        Task<List<Account>> GetByCustomerIdAsync(int customerId);
        Task<List<Account>> GetAllAsync();
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(Account account);
        Task SaveAsync();
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _db;
        public AccountRepository(ApplicationDbContext db) => _db = db;

        public async Task<Account?> GetByIdAsync(int id) =>
            await _db.Accounts.Include(a => a.Customer).ThenInclude(c => c.User)
                .Include(a => a.InstantCredits)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Account?> GetByNumberAsync(string accountNumber) =>
            await _db.Accounts.Include(a => a.Customer).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        public async Task<List<Account>> GetByCustomerIdAsync(int customerId) =>
            await _db.Accounts.Include(a => a.InstantCredits)
                .Where(a => a.CustomerId == customerId).ToListAsync();

        public async Task<List<Account>> GetAllAsync() =>
            await _db.Accounts.Include(a => a.Customer).ThenInclude(c => c.User).ToListAsync();

        public async Task AddAsync(Account account) => await _db.Accounts.AddAsync(account);
        public Task UpdateAsync(Account account) { _db.Accounts.Update(account); return Task.CompletedTask; }
        public Task DeleteAsync(Account account) { _db.Accounts.Remove(account); return Task.CompletedTask; }
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }

    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetByAccountIdAsync(int accountId);
        Task<List<Transaction>> GetAllAsync();
        Task<List<Transaction>> GetFilteredAsync(int? accountId, Models.TransactionType? type, DateTime? from, DateTime? to);
        Task AddAsync(Transaction transaction);
        Task SaveAsync();
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _db;
        public TransactionRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<Transaction>> GetByAccountIdAsync(int accountId) =>
            await _db.Transactions.Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<List<Transaction>> GetAllAsync() =>
            await _db.Transactions.Include(t => t.Account).ThenInclude(a => a.Customer).ThenInclude(c => c.User)
                .OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<List<Transaction>> GetFilteredAsync(int? accountId, Models.TransactionType? type, DateTime? from, DateTime? to)
        {
            var q = _db.Transactions.Include(t => t.Account).ThenInclude(a => a.Customer).ThenInclude(c => c.User).AsQueryable();
            if (accountId.HasValue) q = q.Where(t => t.AccountId == accountId);
            if (type.HasValue) q = q.Where(t => t.Type == type);
            if (from.HasValue) q = q.Where(t => t.CreatedAt >= from);
            if (to.HasValue) q = q.Where(t => t.CreatedAt <= to);
            return await q.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task AddAsync(Transaction transaction) => await _db.Transactions.AddAsync(transaction);
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }

    public interface IAccountRequestRepository
    {
        Task<List<AccountRequest>> GetPendingAsync();
        Task<List<AccountRequest>> GetByCustomerIdAsync(int customerId);
        Task<List<AccountRequest>> GetAllAsync();
        Task<AccountRequest?> GetByIdAsync(int id);
        Task AddAsync(AccountRequest request);
        Task UpdateAsync(AccountRequest request);
        Task SaveAsync();
    }

    public class AccountRequestRepository : IAccountRequestRepository
    {
        private readonly ApplicationDbContext _db;
        public AccountRequestRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<AccountRequest>> GetPendingAsync() =>
            await _db.AccountRequests.Include(r => r.Customer).ThenInclude(c => c.User)
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderByDescending(r => r.RequestedAt).ToListAsync();

        public async Task<List<AccountRequest>> GetByCustomerIdAsync(int customerId) =>
            await _db.AccountRequests.Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.RequestedAt).ToListAsync();

        public async Task<List<AccountRequest>> GetAllAsync() =>
            await _db.AccountRequests.Include(r => r.Customer).ThenInclude(c => c.User)
                .OrderByDescending(r => r.RequestedAt).ToListAsync();

        public async Task<AccountRequest?> GetByIdAsync(int id) =>
            await _db.AccountRequests.Include(r => r.Customer).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task AddAsync(AccountRequest request) => await _db.AccountRequests.AddAsync(request);
        public Task UpdateAsync(AccountRequest r) { _db.AccountRequests.Update(r); return Task.CompletedTask; }
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }

    public interface IInstantCreditRepository
    {
        Task<List<InstantCredit>> GetByAccountIdAsync(int accountId);
        Task<List<InstantCredit>> GetAllPendingAsync();
        Task<List<InstantCredit>> GetOverdueAsync();
        Task<InstantCredit?> GetByIdAsync(int id);
        Task AddAsync(InstantCredit credit);
        Task UpdateAsync(InstantCredit credit);
        Task SaveAsync();
    }

    public class InstantCreditRepository : IInstantCreditRepository
    {
        private readonly ApplicationDbContext _db;
        public InstantCreditRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<InstantCredit>> GetByAccountIdAsync(int accountId) =>
            await _db.InstantCredits.Where(ic => ic.AccountId == accountId)
                .OrderByDescending(ic => ic.IssuedAt).ToListAsync();

        public async Task<List<InstantCredit>> GetAllPendingAsync() =>
            await _db.InstantCredits.Include(ic => ic.Account).ThenInclude(a => a.Customer).ThenInclude(c => c.User)
                .Where(ic => ic.Status == CreditStatus.PendingSettlement)
                .OrderByDescending(ic => ic.IssuedAt).ToListAsync();

        public async Task<List<InstantCredit>> GetOverdueAsync() =>
            await _db.InstantCredits.Include(ic => ic.Account)
                .Where(ic => ic.Status == CreditStatus.PendingSettlement && ic.Deadline < DateTime.UtcNow)
                .ToListAsync();

        public async Task<InstantCredit?> GetByIdAsync(int id) =>
            await _db.InstantCredits.Include(ic => ic.Account).ThenInclude(a => a.Customer).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(ic => ic.Id == id);

        public async Task AddAsync(InstantCredit credit) => await _db.InstantCredits.AddAsync(credit);
        public Task UpdateAsync(InstantCredit ic) { _db.InstantCredits.Update(ic); return Task.CompletedTask; }
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }

    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<List<AuditLog>> GetRecentAsync(int count);
        Task SaveAsync();
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _db;
        public AuditLogRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(AuditLog log) => await _db.AuditLogs.AddAsync(log);
        public async Task<List<AuditLog>> GetRecentAsync(int count) =>
            await _db.AuditLogs.OrderByDescending(l => l.Timestamp).Take(count).ToListAsync();
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }
}
