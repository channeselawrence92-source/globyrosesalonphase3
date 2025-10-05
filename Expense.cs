/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System;

namespace SalonManager.Models
{
    public class Expense
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Category { get; set; } = "";   // Rent, Utilities, Supplies...
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public string Vendor { get; set; } = "";

        public Expense() { }

        public Expense(string category, decimal amount, DateTime dueDate, string vendor = "", bool isPaid = false)
        {
            Category = category;
            Amount = amount;
            DueDate = dueDate;
            Vendor = vendor;
            IsPaid = isPaid;
        }

        public override string ToString() =>
            $"{DueDate:d} {(IsPaid ? "[PAID]" : "[DUE]")} {Category} {Amount:C} {(string.IsNullOrWhiteSpace(Vendor) ? "" : $"@ {Vendor}")}";
    }
}
