## Project Summary

### Project Name
**Glo by Rose — Salon Manager**

### Project Description
Glo by Rose — Salon Manager is a .NET 6 console application that helps independent salon owners manage booth renters, payments, expenses, and compliance deadlines (e.g., DPOR licenses). The app persists data in SQLite, provides a monthly income vs. expense summary, and shows 30-day compliance alerts so nothing slips through the cracks.

---

### Project Tasks
- **Task 1: Set up the development environment**
  - Install .NET 6 SDK
  - Initialize Git and GitHub repository
- **Task 2: Design the application (OOP)**
  - Define models: `Person` (abstract), `Renter : Person`, `Payment`, `Expense`, `ComplianceItem`
  - Plan console menu flows and report outputs
- **Task 3: Implement storage**
  - Create `IStorageService` abstraction
  - Implement `SqliteStorageService` with tables for Renters, Payments, Expenses, ComplianceItems
  - Ensure CRUD and foreign keys; serialize/deserialize types and dates
- **Task 4: Build the console UI**
  - `Menu` class with options to add/list renters, record payments, add expenses
  - Add “Compliance Alerts (30 days)” and “Add Compliance Item”
- **Task 5: Reports**
  - `ReportService` to compute monthly income/expenses and expiring compliance items
- **Task 6: Input/Output & Validation**
  - Prompt/parse input with guards; format currency and dates; friendly messages
- **Task 7: Test the application**
  - Run flows end-to-end: add renter → record payment → add expense → monthly summary → compliance alert
- **Task 8: Documentation & tagging**
  - Write README with overview, setup, usage, and Project Summary
  - Tag release in GitHub as **“Phase #3”**

---

### Project Skills Learned
- C# OOP: abstraction, inheritance/polymorphism, composition, constructors, access modifiers  
- Data persistence with SQLite using `Microsoft.Data.Sqlite`  
- Console UI/UX and input validation  
- Separation of concerns (Models / Services / Menus)  
- Git & GitHub workflows (commits, tags, releases)

---

### Language / Tech Used
- **C# / .NET 6** (console app)  
- **SQLite** (local file database)  
- **Microsoft.Data.Sqlite** (data access library)  
- **Git & GitHub** (version control, tagging)

---

### Development Process Used
Iterative development with small, testable increments:
- Design models → storage → menu flows → reports → polish/validation  
- Frequent local runs and refactors based on rubric acceptance criteria

---

### Setup Notes
Install .NET 6 SDK, then from the project root:

```bash
dotnet restore
dotnet build
dotnet run
