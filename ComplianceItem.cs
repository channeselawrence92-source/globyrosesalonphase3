/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System;

namespace SalonManager.Models
{
    public class ComplianceItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = ""; // Health Inspection, Fire Inspection, License, Insurance
        public string Notes { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsValid { get; set; } = true;

        public ComplianceItem() { }

        public ComplianceItem(string type, DateTime issue, DateTime expiration, string notes = "", bool isValid = true)
        {
            Type = type;
            IssueDate = issue;
            ExpirationDate = expiration;
            Notes = notes;
            IsValid = isValid;
        }

        public bool IsExpiringSoon(int days = 30)
            => IsValid && DateTime.Now.AddDays(days) >= ExpirationDate;

        public override string ToString()
            => $"{Type}: Issued {IssueDate:d}, Expires {ExpirationDate:d} {(IsValid ? "[VALID]" : "[INVALID]")} {(IsExpiringSoon() ? "⚠️ Expiring soon" : "")}";
    }
}
