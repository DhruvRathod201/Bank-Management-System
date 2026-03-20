# 🏰 NEXBank — Premium Bank Management System

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

A sophisticated full-stack ASP.NET Core 8 MVC web application simulating a high-end banking platform. Featuring dedicated **Admin** and **Customer** portals with a focus on security, elegant design, and robust business logic.

---

## ✨ Key Features

### 👤 Customer Portal
- **Instant Credit:** Get temporary credit up to ₹10,000 instantly.
- **Secure Transfers:** Move money between accounts with password verification.
- **Account Management:** Open Savings, Current, or FD accounts with ease.
- **Digital Passbook:** Detailed transaction history and real-time balance updates.
- **Premium UI:** A "Château" design aesthetic for a luxurious banking experience.

### 🛡️ Admin Portal
- **Customer Oversight:** Approve, block, or manage customer profiles.
- **Request Processing:** Review and approve account opening requests.
- **Financial Control:** Settle instant credits, freeze accounts, and issue admin credits.
- **Audit Logging:** Comprehensive tracking of all administrative actions.
- **Detailed Reporting:** Real-time stats on customers, accounts, and transactions.

---

## Table of Contents

1. [Tech Stack](#tech-stack)
2. [Project Structure](#project-structure)
3. [How the Application Flows](#how-the-application-flows)
4. [Database & Models](#database--models)
5. [Services Layer](#services-layer)
6. [Controllers](#controllers)
7. [Views](#views)
8. [Business Rules](#business-rules)
9. [Setup & Run Instructions](#setup--run-instructions)
10. [Login Credentials](#login-credentials)
11. [Common Errors & Fixes](#common-errors--fixes)

---

## Tech Stack

| Layer        | Technology                          |
|--------------|-------------------------------------|
| Framework    | ASP.NET Core 8 MVC                  |
| Database     | SQL Server (Express or LocalDB)     |
| ORM          | Entity Framework Core 8             |
| Auth         | Cookie-based authentication (built-in) |
| Passwords    | BCrypt.Net hashing                  |
| Frontend     | Bootstrap 5 + Bootstrap Icons       |
| Fonts        | Cinzel + EB Garamond (Google Fonts) |
| Logging      | Serilog (file-based, /Logs/)        |

---

## Project Structure

```
BankManagementSystem_Updated/
│
├── Controllers/
│   ├── HomeController.cs         ← Landing page (/)
│   ├── AuthController.cs         ← Register, Login, Logout
│   ├── CustomerController.cs     ← All customer actions
│   └── AdminController.cs        ← All admin actions
│
├── Models/
│   ├── User.cs                   ← Login credentials + role
│   ├── Customer.cs               ← Customer profile linked to User
│   ├── Account.cs                ← Bank account (Savings/Current/FD)
│   ├── AccountRequest.cs         ← Request to open an account
│   ├── Transaction.cs            ← Every money movement
│   ├── InstantCredit.cs          ← Temporary credit tracking
│   └── AuditLog.cs               ← Admin action history
│
├── Services/
│   ├── AuthService.cs            ← Register/Login/Logout logic
│   ├── AccountService.cs         ← Account CRUD + approve/reject requests
│   ├── TransactionService.cs     ← Deposit/Withdraw/Transfer logic
│   └── AdminService.cs           ← Customer management, credit settlement, overdue processing
│
├── Data/
│   ├── ApplicationDbContext.cs   ← EF Core DbContext (all tables)
│   └── Repositories/
│       └── Repositories.cs       ← All repository interfaces + implementations
│
├── DTOs/
│   └── Dtos.cs                   ← Data Transfer Objects (form models)
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml        ← Base layout for auth/home pages
│   │   ├── _AdminLayout.cshtml   ← Sidebar layout for admin pages
│   │   └── _CustomerLayout.cshtml← Sidebar layout for customer pages
│   ├── Home/
│   │   └── Index.cshtml          ← Landing page
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Admin/
│   │   ├── Dashboard.cshtml
│   │   ├── Customers.cshtml
│   │   ├── CustomerDetails.cshtml
│   │   ├── PendingRequests.cshtml← All account requests (pending + history)
│   │   ├── RequestDetails.cshtml
│   │   ├── Accounts.cshtml
│   │   ├── AdminCredit.cshtml    ← Admin-only: add money to any account
│   │   ├── InstantCredits.cshtml
│   │   ├── Transactions.cshtml
│   │   └── Reports.cshtml
│   └── Customer/
│       ├── Dashboard.cshtml
│       ├── Accounts.cshtml       ← Shows pending + rejected requests
│       ├── AccountDetails.cshtml
│       ├── OpenAccount.cshtml
│       ├── OpenAccountForm.cshtml
│       ├── Deposit.cshtml        ← Instant Credit form
│       ├── Withdraw.cshtml
│       ├── Transfer.cshtml
│       ├── Transactions.cshtml
│       └── Profile.cshtml
│
├── Migrations/                   ← Auto-generated EF Core migrations
├── Logs/                         ← Serilog daily log files
├── wwwroot/
│   ├── css/site.css              ← Full NEXBank Château design system
│   └── js/site.js                ← Alert auto-dismiss + helpers
├── appsettings.json              ← DB connection string + admin seed credentials
└── Program.cs                    ← App startup, DI registration, admin seeding
```

---

## How the Application Flows

### Full User Journey

```
1. CUSTOMER REGISTERS
   └─ POST /Auth/Register
   └─ Creates User (role=Customer, IsActive=true)
   └─ Creates Customer (Status=Pending)
   └─ Cannot log in yet

2. ADMIN APPROVES CUSTOMER
   └─ Admin → Customers → Approve
   └─ Customer.Status → Active
   └─ Customer can now log in

3. CUSTOMER LOGS IN
   └─ POST /Auth/Login
   └─ Checks: IsActive, CustomerStatus != Blocked/Pending/Rejected
   └─ Issues cookie session

4. CUSTOMER REQUESTS AN ACCOUNT
   └─ Customer → Open Account → choose type → fill form
   └─ Creates AccountRequest (Status=Pending)
   └─ Customer sees "Pending" alert on Dashboard & Accounts page

5. ADMIN APPROVES ACCOUNT REQUEST
   └─ Admin → Pending Requests → Approve
   └─ Creates Account with ₹10,000 opening balance
   └─ AccountRequest.Status → Approved

   OR ADMIN REJECTS:
   └─ Admin enters rejection reason
   └─ AccountRequest.Status → Rejected
   └─ Customer sees rejection reason on Dashboard & Accounts page

6. CUSTOMER USES ACCOUNT
   ├─ Instant Credit (Deposit)
   │   └─ Bank credits account temporarily (max ₹10,000)
   │   └─ Creates InstantCredit record (Status=PendingSettlement)
   │   └─ Customer must repay physically within 3 days
   │
   ├─ Withdraw
   │   └─ Requires password confirmation
   │   └─ Must keep ₹10,000 minimum balance
   │   └─ Cannot withdraw from Frozen/Closed accounts
   │
   └─ Transfer
       └─ Requires password confirmation
       └─ Must keep ₹10,000 minimum balance
       └─ Receiver account must exist and be Active
       └─ Shows error if account number not found

7. ADMIN SETTLES INSTANT CREDIT
   └─ Admin → Instant Credits → Settle
   └─ Deducts credit amount from balance
   └─ If balance goes below ₹10,000 — allowed (admin override)
   └─ If balance goes to 0 — capped at 0

8. OVERDUE CREDITS (auto-processed)
   └─ On Dashboard load or InstantCredits page load
   └─ Any credit past deadline → Status=Overdue
   └─ Amount auto-deducted from balance
   └─ Account → Frozen automatically

9. ADMIN MANAGES ACCOUNTS
   ├─ Freeze account (customer cannot withdraw/transfer/instant credit)
   ├─ Unfreeze account
   ├─ Close account
   ├─ Reopen closed account (back to Active)
   ├─ Admin Credit (only way to add money — even to frozen accounts)
   └─ Delete account (permanent, cannot undo)

10. ADMIN BLOCKS CUSTOMER
    └─ Customer.Status → Blocked
    └─ User.IsActive → false
    └─ Customer immediately cannot log in
    └─ Admin can Unblock → restores both
```

---

## Database & Models

### Users table
Stores login credentials. Every person (admin + customer) is a User.
- `Id, FullName, Email, PasswordHash, Role, IsActive, CreatedAt`
- `Role` is either `"Admin"` or `"Customer"`
- `IsActive = false` → cannot log in (used when blocked)

### Customers table
Extended profile for Customer-role users. Linked 1:1 to Users.
- `Id, UserId (FK), Phone, DateOfBirth, Gender, Address, Status, RegisteredAt`
- `Status`: Pending → Active → Blocked / Rejected

### Accounts table
A customer can have multiple accounts.
- `Id, AccountNumber, CustomerId (FK), AccountType, Balance, Status, CreatedAt`
- `AccountType`: Savings, Current, FixedDeposit
- `Status`: Active, Frozen, Closed

### AccountRequests table
Tracks requests to open an account before admin approval.
- `Id, CustomerId, AccountType, Status, RequestedAt, AdminNotes, ...form fields`
- `Status`: Pending → Approved / Rejected
- `AdminNotes` = rejection reason shown to customer

### Transactions table
Every money movement is recorded here.
- `Id, AccountId, Type, Amount, Description, CreatedAt, RelatedAccountNumber`
- `Type`: Deposit, Withdraw, Transfer, InstantCredit, CreditSettlement

### InstantCredits table
Tracks temporary credit loans.
- `Id, AccountId, Amount, Status, IssuedAt, Deadline, SettledAt`
- `Status`: PendingSettlement → Settled / Overdue

### AuditLogs table
Admin actions are logged here for accountability.
- `Id, Action, Timestamp`

---

## Services Layer

All business logic lives in Services. Controllers only call services — no logic in controllers.

### AuthService
- `RegisterAsync` — creates User + Customer, returns error if email taken
- `LoginAsync` — verifies password, checks IsActive + CustomerStatus, issues cookie
- `LogoutAsync` — signs out cookie
- `VerifyPasswordAsync` — used by Withdraw/Transfer to confirm password

### AccountService
- `RequestAccountAsync` — creates AccountRequest
- `ApproveRequestAsync` — creates Account with ₹10,000 seed balance, logs opening deposit
- `RejectRequestAsync` — saves rejection reason in AdminNotes
- `FreezeAccountAsync / UnfreezeAccountAsync / CloseAccountAsync / ReopenAccountAsync`
- `AdminCreditAsync` — admin-only deposit, works on any account including frozen
- `GetRequestsByCustomerIdAsync` — used to show customer their own request history

### TransactionService
Business rules enforced here:
- `DepositAsync` — frozen/closed accounts blocked; max ₹10,000; whole numbers only; one pending credit limit
- `WithdrawAsync` — frozen/closed blocked; minimum balance enforced; whole numbers only; detailed error with max withdrawable
- `TransferAsync` — validates receiver account exists; frozen receiver blocked; minimum balance enforced; whole numbers only

### AdminService (CustomerService + AdminService + InstantCreditService in one file)
- `BlockCustomerAsync` — sets both `Customer.Status=Blocked` AND `User.IsActive=false`
- `UnblockCustomerAsync` — restores both
- `SettleAsync` — deducts credit from balance (capped at 0, not negative); unfreezes if balance recovers
- `ProcessOverdueCreditsAsync` — auto-runs on dashboard/instant credit page load; freezes accounts

---

## Controllers

### HomeController
Just shows the landing page (`/`).

### AuthController
- `GET /Auth/Login` + `POST` — handles login
- `GET /Auth/Register` + `POST` — handles registration
- `GET /Auth/Logout` — signs out

### CustomerController
Every action checks the customer is not Blocked (`GetActiveCustomerAsync()`).
Key routes: `Dashboard, Accounts, AccountDetails, OpenAccount, OpenAccountForm, Deposit, Withdraw, Transfer, Transactions, Profile`

### AdminController
All routes require `[Authorize(Roles = "Admin")]`.
Key routes: `Dashboard, Customers, CustomerDetails, PendingRequests, RequestDetails, Accounts, AdminCredit, Transactions, InstantCredits, Reports`

---

## Views

### Layout system
- `_Layout.cshtml` — bare layout (only Bootstrap + site.css). Used by Login, Register, Home.
- `_AdminLayout.cshtml` — full sidebar layout for admin. Sidebar has gold accents.
- `_CustomerLayout.cshtml` — full sidebar layout for customer.

### Design system (wwwroot/css/site.css)
The Château design uses:
- **Colors**: Burgundy `#6B1A2A`, Gold `#C9A84C`, Velvet `#1A0A0F`, Cream `#FAF6F0`
- **Fonts**: Cinzel (headings/labels), EB Garamond (body text)
- **Classes**: `.auth-bg-dark`, `.auth-card`, `.auth-logo-seal`, `.ornament`, `.stat-card`, `.stat-blue/green/orange/red`

---

## Business Rules

| Rule | Where enforced |
|------|---------------|
| Minimum balance ₹10,000 on all withdrawals/transfers | `TransactionService.WithdrawAsync / TransferAsync` |
| Maximum instant credit ₹10,000 | `TransactionService.DepositAsync` |
| Only one pending instant credit at a time | `TransactionService.DepositAsync` |
| Frozen account: no instant credit, no withdraw, no transfer | `TransactionService` |
| Frozen account: admin can still credit it | `AccountService.AdminCreditAsync` |
| Closed account: no operations | `TransactionService` |
| Closed account: admin can reopen | `AccountService.ReopenAccountAsync` |
| Blocked customer: cannot log in at all | `AuthService.LoginAsync` |
| Overdue instant credit: auto-freezes account | `InstantCreditService.ProcessOverdueCreditsAsync` |
| Settlement never makes balance negative | `InstantCreditService.SettleAsync` (capped at 0) |
| All amounts must be whole numbers | `TransactionService` + HTML `step=1` + JS floor |
| Receiver account must exist and be active for transfers | `TransactionService.TransferAsync` |
| Admin credit is the only external money source | `AccountService.AdminCreditAsync` |

---

## Setup & Run Instructions

### Prerequisites
- .NET 8 SDK — https://dotnet.microsoft.com/download
- SQL Server Express — install via VS installer or standalone
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### Steps

```bash
# 1. Clone or Extract the project
# Navigate to the project root
cd BankManagementSystem

# 2. Set your connection string in appsettings.json
#    SQL Server Express (default):
#    "Server=.\\SQLEXPRESS;Database=BankManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# 3. Restore packages
dotnet restore

# 4. Drop old database if it exists
dotnet ef database drop --force

# 5. Delete old migration files
#    Delete everything in the Migrations/ folder
del Migrations\*

# 6. Create fresh migration
dotnet ef migrations add InitialCreate

# 7. Apply migration (creates database + tables + admin user)
dotnet ef database update

# 8. Run
dotnet run
```

Open: `http://localhost:5000`

### Testing Admin + Customer simultaneously
Since sessions are cookie-based, two browser tabs share the same session.

**Solution:** Open admin in normal browser, open customer in **InPrivate / Incognito** window.

---

## 📸 Screenshots

> [!TIP]
> Add screenshots of the Admin Dashboard and Customer Portal here to showcase the "Château" design!

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

## 📞 Support

If you have any questions or need help setting up, feel free to open an issue or contact the maintainers.

---

## Common Errors & Fixes

| Error | Fix |
|-------|-----|
| `dotnet` not recognized | Install .NET 8 SDK and restart terminal |
| `dotnet ef` not recognized | Run `dotnet tool install --global dotnet-ef` |
| `Invalid object name 'Users'` | Run `dotnet ef database update` again |
| Cannot connect to database | Check connection string in `appsettings.json` |
| `CS0103 keyframes` compile error | CSS `@keyframes` must be in `.css` files, not inline in `.cshtml` |
| Build failed | Run `dotnet build` to see specific error lines |
| Port already in use | Edit `Properties/launchSettings.json` and change the port |
| Migration conflict | Delete all files in `Migrations/` and redo from step 6 |
| Balance becomes negative after settlement | Fixed — settlement is now capped at 0 |
| Blocked user can still log in | Fixed — `User.IsActive` is now checked at login |
