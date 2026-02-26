# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

**Run the app:**
```bash
dotnet run
```

**Build:**
```bash
dotnet build
```

**Add a new EF Core migration:**
```bash
dotnet ef migrations add <MigrationName>
```

**Apply migrations (CLI):**
```bash
dotnet ef database update
```

**Apply migrations (Visual Studio Package Manager Console):**
```
Update-Database
```

## Architecture

### Two DbContexts

The app uses two EF Core contexts that share one SQL Server database (`BeanScene2027` on `localhost\SQLEXPRESS`):

- **`BeanSceneContext`** — domain tables: `Area`, `Reservation`, `SittingSchedule`, `RestaurantTable`, `ReservationTable` (many-to-many join)
- **`ApplicationDbContext`** — Identity tables only, extends `IdentityDbContext<ApplicationUser>`

### Domain Model

- **`SittingSchedule`** — a bookable time window (e.g. Breakfast/Lunch/Dinner) with a capacity and `Open`/`Closed` status
- **`Reservation`** — belongs to a `SittingSchedule`; status flows `Pending` → `Confirmed`
- **`ReservationTable`** — join table between `Reservation` and `RestaurantTable` (composite PK)
- **`RestaurantTable`** — belongs to an `Area`, has a seat count
- **`Area`** — groups tables by location

### Role-Based Access Control

Three roles seeded at startup (`Data/SeedIdentity.cs`): **Admin**, **Staff**, **Member**.

- **Admin** — full access including user management (`AdminController`: list users, assign roles, delete accounts)
- **Staff** — can manage all reservations and assign tables
- **Member** — can create reservations and view/cancel only their own (matched by email address)

Identity requires email confirmation (`RequireConfirmedAccount = true`).

### Reservation Workflow

1. **Create** (`ReservationsController.Create`) — validates sitting is Open and capacity not exceeded; sends a confirmation email.
2. **Assign Tables** (`ReservationsController.AssignTables`, Admin/Staff only) — checks for double-booking within the same sitting and that total table seats ≥ guest count; sets status to `Confirmed` and sends another email.

### Configuration & Secrets

- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- SMTP host/port/user: `appsettings.json` → `Smtp:*`
- SMTP password: stored in **User Secrets** (not in source). To set it:
  ```bash
  dotnet user-secrets set "Smtp:Password" "<your-app-password>"
  ```

### Other Components

- **SignalR** — `ChatHub` mapped to `/chathub`; `ChatController` serves the chat page
- **Blazor Server** — `ImageGenerator` component mounted via `MapRazorComponents` (mixed MVC + Blazor setup)


## Working rules
- Make small, safe, incremental changes (commit per folder/feature area).
- Do not change runtime behavior unless explicitly asked.
- Prefer deleting redundant comments over rewriting logic.
- After each change set: `dotnet build` (and `dotnet test` if present).
- Avoid large-scale formatting or renaming across the entire solution.

## Refactor Safety Rule

Before and after any non-trivial refactor:
- Run `dotnet build`
- Confirm no compile errors
- Re-check the manual steps in docs/verification-checklist.md
- Do not change reservation behavior unless explicitly asked.