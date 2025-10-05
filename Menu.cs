/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System;
using System.Globalization;
using System.Linq;
using SalonManager.Models;
using SalonManager.Services;

namespace SalonManager.Menus
{
    public static class Menu
    {
        public static void Run(AppState state, IStorageService storage)
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n=== Glo by Rose – Salon Manager ===");
                Console.WriteLine("1) Add Renter");
                Console.WriteLine("2) List Renters");
                Console.WriteLine("3) Record Payment");
                Console.WriteLine("4) Add Expense");
                Console.WriteLine("5) Monthly Summary");
                Console.WriteLine("6) Compliance Alerts (30 days)");
                Console.WriteLine("7) Save & Exit");
                Console.WriteLine("8) Add Compliance Item");



                Console.Write("Choose: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddRenter(state);
                        break;
                    case "2":
                        ListRenters(state);
                        break;
                    case "3":
                        RecordPayment(state);
                        break;
                    case "4":
                        AddExpense(state);
                        break;
                    case "5":
                        MonthlySummary(state);
                        break;
                    case "6":
                        ComplianceAlerts(state);
                        break;
                    case "7":
                        storage.SaveAsync(state).GetAwaiter().GetResult();
                        Console.WriteLine("Saved. Bye!");
                        exit = true;
                        break;
                    case "8":
                        AddCompliance(state);
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        private static void AddRenter(AppState state)
        {
            Console.Write("Name: ");
            var name = Console.ReadLine() ?? "";
            Console.Write("Booth #: ");
            int booth = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Rate (e.g., 200): ");
            decimal rate = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture);
            Console.Write("Billing Frequency (Weekly/Monthly): ");
            var freq = Console.ReadLine() ?? "Monthly";
            Console.Write("Start Date (yyyy-MM-dd): ");
            var start = DateTime.Parse(Console.ReadLine() ?? DateTime.Today.ToString("yyyy-MM-dd"));

            var renter = new Renter(name, booth, rate, freq, start);
            state.Renters.Add(renter);
            Console.WriteLine("Renter added.");
        }

        private static void ListRenters(AppState state)
        {
            if (!state.Renters.Any()) { Console.WriteLine("No renters yet."); return; }
            foreach (var r in state.Renters) Console.WriteLine($"{r.Id} | {r}");
        }

        private static void RecordPayment(AppState state)
        {
            if (!state.Renters.Any()) { Console.WriteLine("Add a renter first."); return; }
            Console.Write("Renter Id: ");
            if (!Guid.TryParse(Console.ReadLine(), out var renterId)) { Console.WriteLine("Bad Id."); return; }
            var renter = state.Renters.FirstOrDefault(x => x.Id == renterId);
            if (renter == null) { Console.WriteLine("Renter not found."); return; }

            Console.Write("Amount: ");
            var amt = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture);
            Console.Write("Date (yyyy-MM-dd) or blank for today: ");
            var dateStr = Console.ReadLine();
            var paidDate = string.IsNullOrWhiteSpace(dateStr) ? DateTime.Now : DateTime.Parse(dateStr!);
            Console.Write("Method: ");
            var method = Console.ReadLine() ?? "";
            Console.Write("Notes: ");
            var notes = Console.ReadLine() ?? "";

            renter.PaymentHistory.Add(new Payment(amt, paidDate, method, notes));
            Console.WriteLine("Payment recorded.");
        }

        private static void AddExpense(AppState state)
        {
            Console.Write("Category: ");
            var cat = Console.ReadLine() ?? "";
            Console.Write("Amount: ");
            var amt = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture);
            Console.Write("Due Date (yyyy-MM-dd): ");
            var due = DateTime.Parse(Console.ReadLine() ?? DateTime.Today.ToString("yyyy-MM-dd"));
            Console.Write("Vendor (optional): ");
            var vendor = Console.ReadLine() ?? "";
            state.Expenses.Add(new Expense(cat, amt, due, vendor, isPaid: false));
            Console.WriteLine("Expense added.");
        }

        private static void MonthlySummary(AppState state)
        {
            Console.Write("Month (1-12): ");
            int month = int.Parse(Console.ReadLine() ?? "1");
            Console.Write("Year (e.g., 2025): ");
            int year = int.Parse(Console.ReadLine() ?? DateTime.Now.Year.ToString());

            var report = new ReportService(state.Renters, state.Expenses, state.ComplianceItems);
            var (income, expenses) = report.GetMonthlySummary(month, year);
            Console.WriteLine($"Income: {income:C} | Expenses: {expenses:C} | Net: {(income - expenses):C}");
        }

        private static void ComplianceAlerts(AppState state)
        {
            var report = new ReportService(state.Renters, state.Expenses, state.ComplianceItems);
            var alerts = report.GetComplianceAlerts(30);
            if (!alerts.Any()) { Console.WriteLine("No compliance items expiring in 30 days."); return; }
            foreach (var c in alerts) Console.WriteLine(c);
        }
        // Optional helper to allow "c" / "cancel"
        static bool IsCancel(string? s) =>
            !string.IsNullOrWhiteSpace(s) &&
            (s.Equals("c", StringComparison.OrdinalIgnoreCase) || s.Equals("cancel", StringComparison.OrdinalIgnoreCase));

        private static void AddCompliance(AppState state)
        {
            Console.Write("Type (e.g., DPOR License) [blank/c to cancel]: ");
            var type = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(type) || IsCancel(type)) { Console.WriteLine("Canceled."); return; }

            // Issue date (optional, Enter = today)
            DateTime issue;
            while (true)
            {
                Console.Write("Issue Date (yyyy-MM-dd) [Enter=today, c=cancel]: ");
                var s = Console.ReadLine();
                if (IsCancel(s)) { Console.WriteLine("Canceled."); return; }
                if (string.IsNullOrWhiteSpace(s)) { issue = DateTime.Today; break; }
                if (DateTime.TryParse(s, out issue)) break;
                Console.WriteLine("Invalid date. Try again.");
            }

            // Expiration date (required)
            DateTime expiration;
            while (true)
            {
                Console.Write("Expiration Date (yyyy-MM-dd) [required, c=cancel]: ");
                var s = Console.ReadLine();
                if (IsCancel(s)) { Console.WriteLine("Canceled."); return; }
                if (!string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out expiration)) break;
                Console.WriteLine("Invalid date. Try again.");
            }

            Console.Write("Notes (optional) [blank/c to skip]: ");
            var notes = Console.ReadLine();
            if (IsCancel(notes)) notes = "";

            state.ComplianceItems.Add(new ComplianceItem(type!, issue, expiration, notes ?? "", isValid: true));
            Console.WriteLine("Compliance item added. (Tip: pick an expiration within 30 days to see it under option 6.)");
        }

    }
}
