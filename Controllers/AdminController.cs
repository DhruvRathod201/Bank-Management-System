using BankManagementSystem.DTOs;
using BankManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _admin;
        private readonly ICustomerService _customers;
        private readonly IAccountService _accounts;
        private readonly ITransactionService _transactions;
        private readonly IInstantCreditService _credits;

        public AdminController(IAdminService admin, ICustomerService customers, IAccountService accounts,
            ITransactionService transactions, IInstantCreditService credits)
        {
            _admin = admin;
            _customers = customers;
            _accounts = accounts;
            _transactions = transactions;
            _credits = credits;
        }

        public async Task<IActionResult> Dashboard()
        {
            var (c, a, t, p) = await _admin.GetDashboardStatsAsync();
            ViewBag.TotalCustomers = c;
            ViewBag.TotalAccounts = a;
            ViewBag.TotalTransactions = t;
            ViewBag.PendingRequests = p;
            return View();
        }

        // ── Customers ──────────────────────────────────
        public async Task<IActionResult> Customers()
        {
            var all = await _customers.GetAllAsync();
            return View(all);
        }

        public async Task<IActionResult> CustomerDetails(int id)
        {
            var customer = await _customers.GetByIdAsync(id);
            if (customer == null) return NotFound();
            var accs = await _accounts.GetCustomerAccountsAsync(id);
            var reqs = await _accounts.GetRequestsByCustomerIdAsync(id);
            ViewBag.Accounts = accs;
            ViewBag.Requests = reqs;
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCustomer(int id)
        {
            var (_, msg) = await _admin.ApproveCustomerAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Customers");
        }

        [HttpPost]
        public async Task<IActionResult> RejectCustomer(int id)
        {
            var (_, msg) = await _admin.RejectCustomerAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Customers");
        }

        [HttpPost]
        public async Task<IActionResult> BlockCustomer(int id)
        {
            var (_, msg) = await _admin.BlockCustomerAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Customers");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockCustomer(int id)
        {
            var (_, msg) = await _admin.UnblockCustomerAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Customers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var (_, msg) = await _admin.DeleteCustomerAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Customers");
        }

        // ── Account Requests ──────────────────────────
        // Shows ALL requests (pending + approved + rejected) for full history
        public async Task<IActionResult> PendingRequests()
        {
            var reqs = await _accounts.GetAllRequestsAsync();
            return View(reqs);
        }

        public async Task<IActionResult> RequestDetails(int id)
        {
            var req = await _accounts.GetRequestByIdAsync(id);
            if (req == null) return NotFound();
            return View(req);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var (success, msg) = await _accounts.ApproveRequestAsync(id);
            TempData[success ? "Success" : "Error"] = msg;
            return RedirectToAction("PendingRequests");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id, string notes)
        {
            var (success, msg) = await _accounts.RejectRequestAsync(id, notes);
            TempData[success ? "Success" : "Error"] = msg;
            return RedirectToAction("PendingRequests");
        }

        // ── Accounts ──────────────────────────────────
        public async Task<IActionResult> Accounts()
        {
            var all = await _accounts.GetAllAccountsAsync();
            return View(all);
        }

        [HttpPost]
        public async Task<IActionResult> FreezeAccount(int id)
        {
            var (_, msg) = await _accounts.FreezeAccountAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Accounts");
        }

        [HttpPost]
        public async Task<IActionResult> UnfreezeAccount(int id)
        {
            var (_, msg) = await _accounts.UnfreezeAccountAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Accounts");
        }

        [HttpPost]
        public async Task<IActionResult> CloseAccount(int id)
        {
            var (_, msg) = await _accounts.CloseAccountAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Accounts");
        }

        [HttpPost]
        public async Task<IActionResult> ReopenAccount(int id)
        {
            var (success, msg) = await _accounts.ReopenAccountAsync(id);
            TempData[success ? "Success" : "Error"] = msg;
            return RedirectToAction("Accounts");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var (_, msg) = await _accounts.DeleteAccountAsync(id);
            TempData["Success"] = msg;
            return RedirectToAction("Accounts");
        }

        // Admin credit — only way to add funds to frozen/any account
        [HttpGet]
        public async Task<IActionResult> AdminCredit(int? accountId)
        {
            var allAccounts = await _accounts.GetAllAccountsAsync();
            ViewBag.AllAccounts = allAccounts;
            ViewBag.SelectedAccountId = accountId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminCredit(int accountId, decimal amount, string note)
        {
            var (success, msg) = await _accounts.AdminCreditAsync(accountId, amount, note);
            TempData[success ? "Success" : "Error"] = msg;
            if (success) return RedirectToAction("Accounts");
            var allAccounts = await _accounts.GetAllAccountsAsync();
            ViewBag.AllAccounts = allAccounts;
            ViewBag.SelectedAccountId = accountId;
            return View();
        }

        // ── Transactions ──────────────────────────────
        public async Task<IActionResult> Transactions(TransactionFilterDto filter)
        {
            var txns = await _transactions.GetFilteredTransactionsAsync(filter);
            var allAccounts = await _accounts.GetAllAccountsAsync();
            ViewBag.Filter = filter;
            ViewBag.AllAccounts = allAccounts;
            return View(txns);
        }

        // ── Instant Credits ───────────────────────────
        public async Task<IActionResult> InstantCredits()
        {
            await _credits.ProcessOverdueCreditsAsync();
            var pending = await _credits.GetAllPendingAsync();
            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> SettleCredit(int id)
        {
            var (success, msg) = await _credits.SettleAsync(id);
            TempData[success ? "Success" : "Error"] = msg;
            return RedirectToAction("InstantCredits");
        }

        [HttpPost]
        public async Task<IActionResult> ExtendDeadline(int id, int days)
        {
            var (success, msg) = await _credits.ExtendDeadlineAsync(id, days);
            TempData[success ? "Success" : "Error"] = msg;
            return RedirectToAction("InstantCredits");
        }

        [HttpPost]
        public async Task<IActionResult> FreezeAccountFromCredit(int accountId)
        {
            var (_, msg) = await _accounts.FreezeAccountAsync(accountId);
            TempData["Success"] = msg;
            return RedirectToAction("InstantCredits");
        }

        // ── Reports ───────────────────────────────────
        public async Task<IActionResult> Reports(DateTime? from, DateTime? to)
        {
            from ??= DateTime.UtcNow.AddDays(-30);
            to ??= DateTime.UtcNow;

            var filter = new TransactionFilterDto { FromDate = from, ToDate = to };
            var txns = await _transactions.GetFilteredTransactionsAsync(filter);

            ViewBag.From = from.Value.ToString("yyyy-MM-dd");
            ViewBag.To = to.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalDeposits = txns.Where(t => t.Type == BankManagementSystem.Models.TransactionType.Deposit || t.Type == BankManagementSystem.Models.TransactionType.InstantCredit).Sum(t => t.Amount);
            ViewBag.TotalWithdrawals = txns.Where(t => t.Type == BankManagementSystem.Models.TransactionType.Withdraw).Sum(t => t.Amount);
            ViewBag.TotalTransfers = txns.Where(t => t.Type == BankManagementSystem.Models.TransactionType.Transfer).Sum(t => t.Amount);
            ViewBag.TransactionCount = txns.Count;
            ViewBag.Transactions = txns;
            return View();
        }
    }
}
