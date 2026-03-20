using BankManagementSystem.DTOs;
using BankManagementSystem.Models;
using BankManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankManagementSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customers;
        private readonly IAccountService _accounts;
        private readonly ITransactionService _transactions;
        private readonly IInstantCreditService _credits;
        private readonly IAuthService _auth;

        public CustomerController(ICustomerService customers, IAccountService accounts,
            ITransactionService transactions, IInstantCreditService credits, IAuthService auth)
        {
            _customers = customers;
            _accounts = accounts;
            _transactions = transactions;
            _credits = credits;
            _auth = auth;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Helper: ensure customer is active (not blocked)
        private async Task<Customer?> GetActiveCustomerAsync()
        {
            var customer = await _customers.GetByUserIdAsync(UserId);
            if (customer == null) return null;
            if (customer.Status == CustomerStatus.Blocked) return null;
            return customer;
        }

        public async Task<IActionResult> Dashboard()
        {
            var customer = await _customers.GetByUserIdAsync(UserId);
            if (customer == null) return RedirectToAction("Logout", "Auth");
            if (customer.Status == CustomerStatus.Blocked)
            {
                TempData["Error"] = "Your account has been blocked. Please contact the bank.";
                return RedirectToAction("Logout", "Auth");
            }

            var customerAccounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
            var recentTxns = new List<Transaction>();
            foreach (var acc in customerAccounts)
            {
                var txns = await _transactions.GetAccountTransactionsAsync(acc.Id);
                recentTxns.AddRange(txns.Take(5));
            }
            recentTxns = recentTxns.OrderByDescending(t => t.CreatedAt).Take(10).ToList();

            await _credits.ProcessOverdueCreditsAsync();

            // Account requests for this customer
            var requests = await _accounts.GetRequestsByCustomerIdAsync(customer.Id);

            ViewBag.Customer = customer;
            ViewBag.Accounts = customerAccounts;
            ViewBag.RecentTransactions = recentTxns;
            ViewBag.AccountRequests = requests;
            return View();
        }

        public async Task<IActionResult> Accounts()
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            var accs = await _accounts.GetCustomerAccountsAsync(customer.Id);
            var requests = await _accounts.GetRequestsByCustomerIdAsync(customer.Id);
            ViewBag.Customer = customer;
            ViewBag.Accounts = accs;
            ViewBag.Requests = requests;
            return View();
        }

        public async Task<IActionResult> AccountDetails(int id)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            var account = await _accounts.GetAccountByIdAsync(id);
            if (account == null || account.CustomerId != customer.Id) return NotFound();

            var txns = await _transactions.GetAccountTransactionsAsync(id);
            ViewBag.Account = account;
            ViewBag.Transactions = txns.Take(30).ToList();
            ViewBag.Customer = customer;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> OpenAccount()
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> OpenAccountForm(string type)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            ViewBag.Type = type;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OpenAccountForm(AccountRequestDto dto)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            var (success, message) = await _accounts.RequestAccountAsync(customer.Id, dto);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Accounts");
        }

        [HttpGet]
        public async Task<IActionResult> Deposit(int? accountId)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            var accs = await _accounts.GetCustomerAccountsAsync(customer.Id);
            ViewBag.Accounts = accs;
            ViewBag.SelectedAccountId = accountId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositDto dto)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                return View(dto);
            }
            var (success, message) = await _transactions.DepositAsync(customer.Id, dto);
            TempData[success ? "Success" : "Error"] = message;
            if (!success)
            {
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                ViewBag.SelectedAccountId = dto.AccountId;
                return View(dto);
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Withdraw(int? accountId)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            var accs = await _accounts.GetCustomerAccountsAsync(customer.Id);
            ViewBag.Accounts = accs;
            ViewBag.SelectedAccountId = accountId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(WithdrawDto dto)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                return View(dto);
            }
            if (!await _auth.VerifyPasswordAsync(UserId, dto.Password))
            {
                TempData["Error"] = "Incorrect password.";
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                ViewBag.SelectedAccountId = dto.AccountId;
                return View(dto);
            }
            var (success, message) = await _transactions.WithdrawAsync(customer.Id, dto);
            TempData[success ? "Success" : "Error"] = message;
            if (!success)
            {
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                ViewBag.SelectedAccountId = dto.AccountId;
                return View(dto);
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Transfer(int? accountId)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            var accs = await _accounts.GetCustomerAccountsAsync(customer.Id);
            ViewBag.Accounts = accs;
            ViewBag.SelectedAccountId = accountId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(TransferDto dto)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                return View(dto);
            }
            if (!await _auth.VerifyPasswordAsync(UserId, dto.Password))
            {
                TempData["Error"] = "Incorrect password.";
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                ViewBag.SelectedAccountId = dto.FromAccountId;
                return View(dto);
            }
            var (success, message) = await _transactions.TransferAsync(customer.Id, dto);
            TempData[success ? "Success" : "Error"] = message;
            if (!success)
            {
                ViewBag.Accounts = await _accounts.GetCustomerAccountsAsync(customer.Id);
                ViewBag.SelectedAccountId = dto.FromAccountId;
                return View(dto);
            }
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Transactions(TransactionFilterDto filter)
        {
            var customer = await GetActiveCustomerAsync();
            if (customer == null) return RedirectToAction("Logout", "Auth");
            var accs = await _accounts.GetCustomerAccountsAsync(customer.Id);
            var customerAccountIds = accs.Select(a => a.Id).ToHashSet();

            List<Transaction> txns;
            if (filter.AccountId.HasValue && customerAccountIds.Contains(filter.AccountId.Value))
                txns = await _transactions.GetFilteredTransactionsAsync(filter);
            else
            {
                txns = new List<Transaction>();
                foreach (var acc in accs)
                {
                    var at = await _transactions.GetAccountTransactionsAsync(acc.Id);
                    txns.AddRange(at);
                }
                txns = txns.OrderByDescending(t => t.CreatedAt).ToList();
            }

            ViewBag.Accounts = accs;
            ViewBag.Transactions = txns;
            ViewBag.Filter = filter;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var customer = await _customers.GetByUserIdAsync(UserId);
            ViewBag.Customer = customer;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Customer = await _customers.GetByUserIdAsync(UserId);
                return View(dto);
            }
            var (success, message) = await _customers.UpdateProfileAsync(UserId, dto);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Profile");
        }
    }
}
