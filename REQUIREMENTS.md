# 1. System Overview

A **Bank Management System (BMS)** is a web application that allows:

* Customers to manage their bank accounts
* Bank staff (admins) to manage customers, accounts, and transactions

The system should support:

* Customer registration
* Account opening
* Deposits (Instant Credit) and withdrawals
* Transfers
* Transaction tracking
* Admin approvals

The system has **two main roles**:

```
Customer
Admin (Bank Staff)
```

---

# 2. Customer (User) Role

A **Customer** is a bank client who owns one or more bank accounts.

A customer can:

```
Register
Login
Request new accounts
View accounts
Deposit money (via Instant Credit)
Withdraw money
Transfer money
View transactions
Update profile
```

But **some actions require admin approval**.

---

# 3. Admin Role

The **Admin** represents bank employees.

Admin can:

```
Approve new customers
Approve account requests
Manage customer profiles
Manage accounts
Monitor transactions
Manage instant credit settlements
Freeze accounts
Generate reports
```

Admin has **full system control**.

---

# 4. Main System Pages

The system should have these main sections:

```
Landing Page
Signup Page
Login Page
Customer Dashboard
Admin Dashboard
```

---

# 5. Signup Flow (Customer Registration)

When a new user wants to join the bank.

Signup form should include:

```
Full Name
Email
Phone Number
Date of Birth
Gender
Password
Confirm Password
```

After submitting:

```
User account created
Status = Pending
```

Customer cannot use banking features yet.

---

# 6. Admin Approval of User

Admin dashboard should show:

```
Pending Customer Requests
```

Each request should display:

```
Name
Email
Phone
Registration Date
```

Admin actions:

```
Approve
Reject
```

If approved:

```
Customer status = Active
```

---

# 7. Customer Dashboard

After login, customers see their dashboard.

Dashboard shows:

```
List of Accounts
Account Balances
Recent Transactions
Instant Credit Alerts
Quick Actions
```

Instant credit alerts example:

```
Outstanding Instant Credit: ₹5000
Settlement Deadline: 2 days remaining
```

If unpaid:

```
Warning message displayed
```

Quick actions:

```
Open New Account
Deposit
Withdraw
Transfer
View Transactions
```

---

# 8. Account Types

Customers can have multiple accounts.

Example:

```
Savings Account
Current Account
Fixed Deposit (FD)
```

One user may have:

```
Savings + Current
Savings + FD
Current only
```

---

# 9. Opening a New Account

Customer clicks:

```
Open New Account
```

First step:

```
Select Account Type
```

Options:

```
Savings
Current
FD
```

After selecting type → system shows a **form specific to that account type**.

---

# 10. Savings Account Form

Fields:

```
Address
Occupation
Monthly Salary
Number of family members
Nominee name
Nominee relation
```

After submission:

```
Account Request Created
Status = Pending Approval
```

---

# 11. Current Account Form

Fields:

```
Business Name
Business Type
Business Address
Annual Income
Number of Employees
```

Request goes to admin.

---

# 12. Fixed Deposit Account Form

Fields:

```
Deposit Amount
Duration
Nominee Name
Nominee Relation
```

Admin must approve.

---

# 13. Admin Account Approval

Admin dashboard must show:

```
Pending Account Requests
```

List columns:

```
Customer Name
Account Type
Request Date
Status
```

Sorted by:

```
Most recent first
```

When admin clicks request:

System shows **full submitted form**.

Admin actions:

```
Approve
Reject
```

If approved:

```
Account created
Account number generated
Status = Active
```

---

# 14. Customer Accounts Page

Customers should see a list of their accounts.

Example table:

```
Account Number
Account Type
Balance
Status
```

User can click an account to view details.

---

# 15. Account Details Page

Displays:

```
Account Number
Account Type
Current Balance
Outstanding Instant Credits
Repayment Deadline
Account Status
Recent Transactions
```

Actions:

```
Deposit
Withdraw
Transfer
View Full History
```

---

# 16. Deposit Flow (Instant Deposit Credit)

The system supports **Instant Deposit Credit**, functioning as a temporary loan from the bank. The customer must later repay physically at the bank branch. Every account must maintain a **minimum balance of ₹10,000** as security for this feature.

**Step 1 — User clicks Deposit**

User selects:

```
Account
Deposit Amount
```

Validation:

```
Amount ≤ 10,000
Amount > 0
Account Status = Active
```

**Step 2 — System Provides Temporary Credit**

System performs:

```
Balance += Amount
Transaction Type = InstantCredit
Status = PendingSettlement
```

This means the bank has temporarily credited the user.

**Settlement Requirement**
The user must repay the credited amount physically at the bank branch within the **settlement deadline** (e.g., 3 days).
Repayment method:
```
Cash deposit at bank
Bank employee verifies payment
Admin marks credit as settled
```

---

# 17. Withdraw Flow

User selects:

```
Account
Amount
```

System asks:

```
Enter password
```

Validation:

```
Amount > 0
Amount ≤ balance
```

If valid:

```
Balance reduced
Transaction recorded
```

---

# 18. Transfer Money

User enters:

```
Receiver Account Number
Amount
```

System checks:

```
Receiver exists
Balance sufficient
```

User confirms password.

System:

```
Debits sender
Credits receiver
Creates two transactions
```

---

# 19. Transaction History

Each account should have a transaction page.

Transaction table columns:

```
Date
Transaction Type
Amount
Account
Description
```

Transaction Types include:

```
Deposit
Withdraw
Transfer
InstantCredit
CreditSettlement
```

Filters:

```
Account
Transaction Type
Date Range
```

Sorting:

```
Newest first
```

---

# 20. Customer Profile Page

Customer can view:

```
Name
Email
Phone
DOB
Address
```

Customer can update:

```
Phone
Address
Password
```

Customer cannot change:

```
Account number
Balance
Account type
```

---

# 21. Admin Dashboard

Admin dashboard should show summary cards:

```
Total Customers
Total Accounts
Total Transactions
Pending Requests
```

Clicking each card opens related page.

---

# 22. Manage Customers (Admin)

Admin can view all customers.

Customer table columns:

```
Customer Name
Email
Phone
Status
Accounts
```

Admin actions:

```
View
Edit
Delete
Block
```

---

# 23. Manage Accounts (Admin)

Admin should see all accounts.

Columns:

```
Account Number
Customer Name
Account Type
Balance
Status
```

Admin actions:

```
View
Freeze
Close
Delete
```

---

# 24. Transaction and Instant Credit Monitoring (Admin)

Admin can view **all transactions**.

Columns:

```
Transaction ID
Customer
Account
Type
Amount
Date
```

Filters:

```
Customer
Account
Transaction Type
Date
```

**Admin Controls for Instant Deposits:**
Admin must be able to view all pending instant deposits.

Admin page should show:

```
Customer Name
Account Number
Credit Amount
Request Date
Deadline
Status
```

Admin actions for Instant Deposits:

```
Mark as Settled
Extend Deadline
Freeze Account
```

---

# 25. Account Freeze Rules

Accounts will be frozen if:

```
Instant credit repayment missed
Fraud suspicion
Admin action
```

If a user does not repay an instant credit within the deadline, the system automatically performs:

```
Deduct credited amount from account balance
Freeze account
```

Frozen accounts can:

```
View balance
Deposit money
```

Frozen accounts cannot:

```
Withdraw
Transfer
Request instant credit
```

---

# 26. Reports (Admin)

Admin can view reports such as:

```
Daily transactions
Total deposits
Total withdrawals
New customers
```

---

# 27. Navigation Layout

Customer sidebar:

```
Dashboard
Accounts
Open Account
Transactions
Profile
Logout
```

Admin sidebar:

```
Dashboard
Customers
Accounts
Transactions
Pending Requests
Instant Credit Requests
Reports
Logout
```

---

# 28. Overall System Flow

```
Customer registers
↓
Admin approves customer
↓
Customer logs in
↓
Customer requests account
↓
Admin approves account
↓
Account becomes active
↓
Customer performs transactions (Withdraw, Transfer, Instant Deposit Credit)
↓
If Instant Deposit used -> Customer repays at branch -> Admin marks settled
↓
Admin monitors system
```

---

# 29. System Architecture

The system should follow a **layered architecture** so responsibilities are clearly separated.

Architecture flow:

```
User Browser
↓
Frontend UI (Razor Views)
↓
Controllers
↓
Services (Business Logic)
↓
Data Access Layer
↓
Database
```

### Layers

**1. Presentation Layer**

Handles UI and user interaction.

Contains:

```
Views
Layouts
Forms
Navigation
```

---

**2. Controller Layer**

Handles HTTP requests.

Responsibilities:

```
Receive user requests
Validate input
Call service layer
Return responses to UI
```

Controllers should be **thin** and contain minimal logic.

---

**3. Service Layer**

Contains business rules and system logic.

Examples:

```
Authentication logic
Account creation logic
Transaction rules
Minimum balance validation
Instant credit rules
Admin approvals
```

---

**4. Data Access Layer**

Handles database operations.

Responsibilities:

```
Saving records
Fetching data
Updating records
Deleting records
```

---

**5. Database Layer**

Stores all system data.

Tables include:

```
Users
Customers
Accounts
Transactions
AccountRequests
InstantCredits
```

---

# 30. Technology Stack

The system should use the following technologies.

Backend:

```
ASP.NET Core MVC (.NET 8)
Entity Framework Core
C#
```

Frontend:

```
Razor Views
Bootstrap 5
HTML
CSS
JavaScript
```

Database:

```
SQL Server Express
```

---

# 31. Authentication System

The system must include **secure authentication**.

Requirements:

```
User login
Admin login
Password hashing
Session management
```

Security rules:

```
Passwords stored as hashes
Sessions expire automatically
Protected routes require login
```

---

# 32. Role-Based Access Control

The system must support **two roles**.

```
Admin
Customer
```

Access rules:

### Customer Access

Customers can access:

```
Dashboard
Accounts
Transactions
Profile
Open account request
Deposit
Withdraw
Transfer
```

Customers cannot access:

```
Admin dashboard
User management
Account approvals
System reports
```

---

### Admin Access

Admins can access:

```
Admin dashboard
Customer management
Account management
Transaction monitoring
Pending account approvals
Instant credit management
Reports
```

Admins cannot perform **customer transactions directly**.

---

# 33. Database Design Requirements

The database must enforce:

```
Primary keys
Foreign keys
Unique constraints
```

Examples:

```
AccountNumber must be unique
User email must be unique
Transactions must reference account
```

Relationships:

```
User → Customer (1:1)
Customer → Accounts (1:Many)
Account → Transactions (1:Many)
Account → InstantCredits (1:Many)
```

---

# 34. Business Logic Rules

These rules must be implemented in the service layer.

### Minimum Balance Rule

```
Minimum balance = ₹10,000
```

Account balance must never fall below this.

---

### Instant Credit Rule

```
Maximum instant credit = ₹10,000
```

Conditions:

```
Account must be active
Credit must not exceed limit
```

---

### Credit Settlement Rule

Instant credit must be repaid within:

```
3 days
```

If unpaid:

```
Account status = Frozen
```

---

# 35. Input Validation

All forms must include validation.

Examples:

Signup validation:

```
Email format valid
Phone number valid
Password strong
Password confirmation matches
```

Transaction validation:

```
Amount must be positive
Amount must not exceed balance
Account must be active
```

---

# 36. Logging and Auditing

The system should log important events.

Events to log:

```
User login
Account creation
Account approval
Transactions
Instant credit requests
Admin actions
```

Logs should include:

```
User
Action
Timestamp
```

---

# 37. Error Handling

The system must gracefully handle errors.

Examples:

```
Invalid login
Account not found
Insufficient balance
Unauthorized access
```

Error messages must be user-friendly.

---

# 38. Performance Requirements

The system should be able to handle:

```
Multiple users simultaneously
Large transaction history
Frequent database queries
```

Optimization methods:

```
Indexed database fields
Efficient queries
Pagination for large tables
```

---

# 39. UI Design Requirements

The UI must be responsive and organized.

Customer navigation:

```
Dashboard
Accounts
Open Account
Transactions
Profile
Logout
```

Admin navigation:

```
Dashboard
Customers
Accounts
Transactions
Pending Requests
Reports
Logout
```

---

# 40. Security Requirements

Security must include:

```
Password hashing
Session authentication
Role-based authorization
Input validation
```

Sensitive actions require confirmation.

Examples:

```
Withdraw
Transfer
Instant credit request
```

User must enter password again for confirmation.

---

# 41. Reporting and Monitoring

Admin should be able to view system statistics.

Examples:

```
Total customers
Total accounts
Total transactions
Pending approvals
```

Reports should support filtering.

Filters:

```
Date range
Customer
Account type
Transaction type
```

---

# 42. Deployment Requirements

The system should run locally using:

```
Windows
.NET Runtime
SQL Server Express
```

Server configuration must include:

```
Database connection string
Migration setup
Environment configuration
```

---

# 43. Testing Requirements

The system must be tested for:

Functional testing:

```
Signup works
Login works
Account approval works
Transactions work
Instant credit logic works
```

Security testing:

```
Unauthorized access blocked
Password protection works
```