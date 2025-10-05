/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SalonManager.Models;

namespace SalonManager.Services
{
    public class SqliteStorageService : IStorageService
    {
        private readonly string _dbPath;
        private readonly string _connStr;

        public SqliteStorageService(string dbPath = "salon.db")
        {
            _dbPath = dbPath;

            // Ensure directory exists if a relative folder is used in the path (e.g., "data/salon.db")
            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _connStr = new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString();
            EnsureCreated();
        }

        private void EnsureCreated()
        {
            
if (!File.Exists(_dbPath)) File.Create(_dbPath).Dispose();

            using var conn = new SqliteConnection(_connStr);
            conn.Open();

            // Enable FK enforcement (needed for ON DELETE CASCADE)
            using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Renters(
  Id TEXT PRIMARY KEY,
  Name TEXT NOT NULL,
  BoothNumber INTEGER NOT NULL,
  Rate REAL NOT NULL,
  BillingFrequency TEXT NOT NULL,
  StartDate TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Payments(
  Id TEXT PRIMARY KEY,
  RenterId TEXT NOT NULL,
  Amount REAL NOT NULL,
  PaidDate TEXT NOT NULL,
  Method TEXT,
  Notes TEXT,
  FOREIGN KEY(RenterId) REFERENCES Renters(Id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS Expenses(
  Id TEXT PRIMARY KEY,
  Category TEXT NOT NULL,
  Amount REAL NOT NULL,
  DueDate TEXT NOT NULL,
  IsPaid INTEGER NOT NULL,
  Vendor TEXT
);
CREATE TABLE IF NOT EXISTS ComplianceItems(
  Id TEXT PRIMARY KEY,
  Type TEXT NOT NULL,
  Notes TEXT,
  IssueDate TEXT NOT NULL,
  ExpirationDate TEXT NOT NULL,
  IsValid INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_Payments_RenterId ON Payments(RenterId);";
            cmd.ExecuteNonQuery();
        }

        public async Task<AppState> LoadAsync()
        {
            var state = new AppState();

            using var conn = new SqliteConnection(_connStr);
            await conn.OpenAsync();

            // Enforce FKs for this connection too
            using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            // Renters
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name, BoothNumber, Rate, BillingFrequency, StartDate FROM Renters;";
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    state.Renters.Add(new Renter
                    {
                        Id = Guid.Parse(r.GetString(0)),
                        Name = r.GetString(1),
                        BoothNumber = r.GetInt32(2),
                        Rate = (decimal)r.GetDouble(3),
                        BillingFrequency = r.GetString(4),
                        StartDate = DateTime.Parse(r.GetString(5)),
                        PaymentHistory = new List<Payment>()
                    });
                }
            }

            // Payments
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, RenterId, Amount, PaidDate, Method, Notes FROM Payments;";
                using var r = await cmd.ExecuteReaderAsync();
                var byRenter = new Dictionary<Guid, List<Payment>>();
                while (await r.ReadAsync())
                {
                    var renterId = Guid.Parse(r.GetString(1));
                    var p = new Payment
                    {
                        Id = Guid.Parse(r.GetString(0)),
                        Amount = (decimal)r.GetDouble(2),
                        PaidDate = DateTime.Parse(r.GetString(3)),
                        Method = r.IsDBNull(4) ? "" : r.GetString(4),
                        Notes = r.IsDBNull(5) ? "" : r.GetString(5)
                    };
                    if (!byRenter.TryGetValue(renterId, out var list))
                    {
                        list = new List<Payment>();
                        byRenter[renterId] = list;
                    }
                    list.Add(p);
                }
                foreach (var renter in state.Renters)
                    if (byRenter.TryGetValue(renter.Id, out var list)) renter.PaymentHistory = list;
            }

            // Expenses
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Category, Amount, DueDate, IsPaid, Vendor FROM Expenses;";
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    state.Expenses.Add(new Expense
                    {
                        Id = Guid.Parse(r.GetString(0)),
                        Category = r.GetString(1),
                        Amount = (decimal)r.GetDouble(2),
                        DueDate = DateTime.Parse(r.GetString(3)),
                        IsPaid = r.GetInt32(4) == 1,
                        Vendor = r.IsDBNull(5) ? "" : r.GetString(5)
                    });
                }
            }

            // Compliance
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Type, Notes, IssueDate, ExpirationDate, IsValid FROM ComplianceItems;";
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    state.ComplianceItems.Add(new ComplianceItem
                    {
                        Id = Guid.Parse(r.GetString(0)),
                        Type = r.GetString(1),
                        Notes = r.IsDBNull(2) ? "" : r.GetString(2),
                        IssueDate = DateTime.Parse(r.GetString(3)),
                        ExpirationDate = DateTime.Parse(r.GetString(4)),
                        IsValid = r.GetInt32(5) == 1
                    });
                }
            }

            return state;
        }

        public async Task SaveAsync(AppState state)
        {
            using var conn = new SqliteConnection(_connStr);
            await conn.OpenAsync();

            // Enforce FKs for this connection as well
            using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            using var tx = conn.BeginTransaction();

            // Upsert Renters
            foreach (var r in state.Renters)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO Renters(Id, Name, BoothNumber, Rate, BillingFrequency, StartDate)
VALUES ($id, $name, $booth, $rate, $freq, $start)
ON CONFLICT(Id) DO UPDATE SET
  Name=$name, BoothNumber=$booth, Rate=$rate, BillingFrequency=$freq, StartDate=$start;";
                cmd.Parameters.AddWithValue("$id", r.Id.ToString());
                cmd.Parameters.AddWithValue("$name", r.Name);
                cmd.Parameters.AddWithValue("$booth", r.BoothNumber);
                cmd.Parameters.AddWithValue("$rate", (double)r.Rate);
                cmd.Parameters.AddWithValue("$freq", r.BillingFrequency);
                cmd.Parameters.AddWithValue("$start", r.StartDate.ToString("O"));
                await cmd.ExecuteNonQueryAsync();

                // Replace Payments for renter (simple approach)
                using (var del = conn.CreateCommand())
                {
                    del.Transaction = tx;
                    del.CommandText = "DELETE FROM Payments WHERE RenterId=$rid;";
                    del.Parameters.AddWithValue("$rid", r.Id.ToString());
                    await del.ExecuteNonQueryAsync();
                }
                foreach (var p in r.PaymentHistory)
                {
                    using var ins = conn.CreateCommand();
                    ins.Transaction = tx;
                    ins.CommandText = @"
INSERT INTO Payments(Id, RenterId, Amount, PaidDate, Method, Notes)
VALUES ($id, $rid, $amt, $date, $method, $notes);";
                    ins.Parameters.AddWithValue("$id", p.Id.ToString());
                    ins.Parameters.AddWithValue("$rid", r.Id.ToString());
                    ins.Parameters.AddWithValue("$amt", (double)p.Amount);
                    ins.Parameters.AddWithValue("$date", p.PaidDate.ToString("O"));
                    ins.Parameters.AddWithValue("$method", p.Method ?? "");
                    ins.Parameters.AddWithValue("$notes", p.Notes ?? "");
                    await ins.ExecuteNonQueryAsync();
                }
            }

            // Replace Expenses
            using (var delE = conn.CreateCommand())
            {
                delE.Transaction = tx;
                delE.CommandText = "DELETE FROM Expenses;";
                await delE.ExecuteNonQueryAsync();
            }
            foreach (var e in state.Expenses)
            {
                using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
INSERT INTO Expenses(Id, Category, Amount, DueDate, IsPaid, Vendor)
VALUES ($id, $cat, $amt, $due, $paid, $vendor);";
                ins.Parameters.AddWithValue("$id", e.Id.ToString());
                ins.Parameters.AddWithValue("$cat", e.Category);
                ins.Parameters.AddWithValue("$amt", (double)e.Amount);
                ins.Parameters.AddWithValue("$due", e.DueDate.ToString("O"));
                ins.Parameters.AddWithValue("$paid", e.IsPaid ? 1 : 0);
                ins.Parameters.AddWithValue("$vendor", e.Vendor ?? "");
                await ins.ExecuteNonQueryAsync();
            }

            // Replace ComplianceItems
            using (var delC = conn.CreateCommand())
            {
                delC.Transaction = tx;
                delC.CommandText = "DELETE FROM ComplianceItems;";
                await delC.ExecuteNonQueryAsync();
            }
            foreach (var c in state.ComplianceItems)
            {
                using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
INSERT INTO ComplianceItems(Id, Type, Notes, IssueDate, ExpirationDate, IsValid)
VALUES ($id, $type, $notes, $issue, $exp, $valid);";
                ins.Parameters.AddWithValue("$id", c.Id.ToString());
                ins.Parameters.AddWithValue("$type", c.Type);
                ins.Parameters.AddWithValue("$notes", c.Notes ?? "");
                ins.Parameters.AddWithValue("$issue", c.IssueDate.ToString("O"));
                ins.Parameters.AddWithValue("$exp", c.ExpirationDate.ToString("O"));
                ins.Parameters.AddWithValue("$valid", c.IsValid ? 1 : 0);
                await ins.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }
    }
}
