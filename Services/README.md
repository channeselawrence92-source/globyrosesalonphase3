# Glo by Rose — Salon Manager (globyrosesalonphase3)

A console app for salon owners to manage renters, record payments, track expenses, and stay ahead of compliance deadlines — with **SQLite** persistence and clean **OOP** design.

---

## Features
- **Renters**: add/list/update/delete; weekly or monthly rates; running balance
- **Payments**: record per renter (amount, date, method, notes)
- **Expenses**: track category, vendor, due date, paid/unpaid
- **Compliance**: items with issue/exp dates + “expiring soon” alerts
- **Monthly Summary**: income vs. expenses for a given month
- **SQLite data store**: creates `salon.db` automatically

---

## Tech Stack
- C# / .NET 6
- SQLite via `Microsoft.Data.Sqlite`
- Console UI

---

## Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

_No separate SQLite install required._

---

## Setup & Run
```bash
dotnet restore
dotnet add package Microsoft.Data.Sqlite
dotnet build
dotnet run
