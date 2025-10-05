/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using SalonManager.Models;

namespace SalonManager.Services
{
    public class ReportService
    {
        private readonly IEnumerable<Renter> _renters;
        private readonly IEnumerable<Expense> _expenses;
        private readonly IEnumerable<ComplianceItem> _compliance;

        public ReportService(IEnumerable<Renter> renters, IEnumerable<Expense> expenses, IEnumerable<ComplianceItem> compliance)
        {
            _renters = renters;
            _expenses = expenses;
            _compliance = compliance;
        }

        public (decimal income, decimal expenses) GetMonthlySummary(int month, int year)
        {
            decimal income = _renters
                .SelectMany(r => r.PaymentHistory)
                .Where(p => p.PaidDate.Month == month && p.PaidDate.Year == year)
                .Sum(p => p.Amount);

            decimal expenses = _expenses
                .Where(e => e.DueDate.Month == month && e.DueDate.Year == year)
                .Sum(e => e.Amount);

            return (income, expenses);
        }

        public List<Expense> GetUpcomingDueDates(int days)
        {
            DateTime cutoff = DateTime.Now.AddDays(days);
            return _expenses
                .Where(e => !e.IsPaid && e.DueDate <= cutoff)
                .OrderBy(e => e.DueDate)
                .ToList();
        }

        public List<ComplianceItem> GetComplianceAlerts(int days = 30)
        {
            return _compliance
                .Where(c => c.IsExpiringSoon(days))
                .OrderBy(c => c.ExpirationDate)
                .ToList();
        }
    }
}
