/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System;

namespace SalonManager.Models
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public DateTime PaidDate { get; set; } = DateTime.Now;
        public string Method { get; set; } = "";  // Cash, Card, Zelle, etc.
        public string Notes { get; set; } = "";

        public Payment() { }

        public Payment(decimal amount, DateTime paidDate, string method, string notes = "")
        {
            Amount = amount;
            PaidDate = paidDate;
            Method = method;
            Notes = notes;
        }

        public override string ToString() => $"{PaidDate:d} {Amount:C} via {Method} {(string.IsNullOrWhiteSpace(Notes) ? "" : $"- {Notes}")}";
    }
}
