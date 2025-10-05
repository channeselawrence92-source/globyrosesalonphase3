/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using SalonManager.Models;

namespace SalonManager.Services
{
    public interface IStorageService
    {
        Task<AppState> LoadAsync();
        Task SaveAsync(AppState state);
    }

    public class AppState
    {
        public List<Renter> Renters { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
        public List<ComplianceItem> ComplianceItems { get; set; } = new();
    }
}
