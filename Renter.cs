/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace SalonManager.Models
{
    public class Renter : Person
    {
        public int BoothNumber { get; set; }
        public decimal Rate { get; set; }
        public string BillingFrequency { get; set; } = "Monthly"; // "Weekly" or "Monthly"
        public DateTime StartDate { get; set; } = DateTime.Today;

        // Composition: payments belong to renter
        public List<Payment> PaymentHistory { get; set; } = new();

        public Renter() { }

        public Renter(string name, int boothNumber, decimal rate, string billingFrequency, DateTime startDate)
        {
            Name = name;
            BoothNumber = boothNumber;
            Rate = rate;
            BillingFrequency = billingFrequency;
            StartDate = startDate;
        }

        public decimal TotalPaid(DateTime? asOf = null)
        {
            var cutoff = asOf ?? DateTime.Now;
            return PaymentHistory.Where(p => p.PaidDate <= cutoff).Sum(p => p.Amount);
        }

        public decimal ExpectedCharges(DateTime? asOf = null)
        {
            var now = asOf ?? DateTime.Now;
            if (now < StartDate) return 0m;

            if (BillingFrequency.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
            {
                var weeks = Math.Max(0, (int)Math.Floor((now - StartDate).TotalDays / 7.0));
                return weeks * Rate;
            }
            else // Monthly
            {
                int months = Math.Max(0, ((now.Year - StartDate.Year) * 12) + now.Month - StartDate.Month);
                return months * Rate;
            }
        }

        public decimal Balance(DateTime? asOf = null) => ExpectedCharges(asOf) - TotalPaid(asOf);

        public override string GetProfile() =>
            $"Renter {Name} – Booth #{BoothNumber}, {BillingFrequency} rate {Rate:C} (since {StartDate:d})";

        public override string ToString() =>
            $"{Name} (Booth {BoothNumber}) | {BillingFrequency} {Rate:C} | Balance: {Balance():C}";
    }
}
