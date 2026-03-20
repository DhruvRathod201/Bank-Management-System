using BankManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BankManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<AccountRequest> AccountRequests => Set<AccountRequest>();
        public DbSet<InstantCredit> InstantCredits => Set<InstantCredit>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Customer -> User (1:1)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId);

            // Account -> Customer (M:1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasColumnType("decimal(18,2)");

            // Transaction -> Account (M:1)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            // AccountRequest -> Customer (M:1)
            modelBuilder.Entity<AccountRequest>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.AccountRequests)
                .HasForeignKey(r => r.CustomerId);

            modelBuilder.Entity<AccountRequest>()
                .Property(r => r.MonthlySalary)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AccountRequest>()
                .Property(r => r.AnnualIncome)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AccountRequest>()
                .Property(r => r.DepositAmount)
                .HasColumnType("decimal(18,2)");

            // InstantCredit -> Account (M:1)
            modelBuilder.Entity<InstantCredit>()
                .HasOne(ic => ic.Account)
                .WithMany(a => a.InstantCredits)
                .HasForeignKey(ic => ic.AccountId);

            modelBuilder.Entity<InstantCredit>()
                .Property(ic => ic.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
