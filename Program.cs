/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System.Threading.Tasks;
using SalonManager.Services;
using SalonManager.Menus;
using SalonManager.Models; 
class Program
{
    static async Task Main()
    {
        IStorageService storage = new SqliteStorageService("salon.db");
        var state = await storage.LoadAsync();

        // Demo seed: one compliance item so option 6 shows something on first run
        if (state.ComplianceItems.Count == 0)
        {
            state.ComplianceItems.Add(new ComplianceItem(
                type: "Health Inspection",
                issue: System.DateTime.Today.AddMonths(-10),
                expiration: System.DateTime.Today.AddDays(20), // shows up in 30-day alerts
                notes: "Annual inspection"
            ));
        }

        Menu.Run(state, storage);
    }
}