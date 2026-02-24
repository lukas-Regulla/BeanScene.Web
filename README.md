# BeanScene Booking System (ASP.NET Core MVC)

## Overview
BeanScene is a full-stack ASP.NET Core MVC web application for managing restaurant bookings and table assignments.
It includes separate experiences for members and admins, with validation rules to prevent invalid or conflicting bookings.

## Tech Stack
- C# / ASP.NET Core MVC
- Razor Views
- Entity Framework Core
- SQL Server (local/dev)
- ASP.NET Core Identity (authentication + roles)
- Git (version control)

## Key Features
- User registration & login (Identity)
- Email account verification
- Booking creation with validation
- Member-only views / member dashboard
- Admin area for user/booking management
- Table assignment workflows (Reservations / RestaurantTables)
- Chat / realtime features (SignalR hubs) *(if enabled in this build)*

## Project Structure (High Level)
- **Controllers/**: MVC controllers (Bookings/Reservations/Admin/Dashboard, etc.)
- **Models/**: domain models + view models
- **Views/**: Razor pages per controller (Admin, Reservations, Dashboard, etc.)
- **Data/**: DbContexts, seed data, Identity setup
- **Migrations/**: EF Core migrations for database schema changes
- **Services/**: email sender + other app services
- **Hubs/**: SignalR hubs (if used)

## Getting Started (Local)
### Prerequisites
- Visual Studio 2022 (Community)
- .NET SDK (matching the project)
- SQL Server / LocalDB

### Run the App
1. Clone the repo
2. Open the `.sln` in Visual Studio
3. Restore NuGet packages (VS usually does this automatically)
4. Apply database migrations:
   - Package Manager Console:
     - `Update-Database`
5. Press **F5** to run

## Configuration Notes
- Connection strings are stored in `appsettings.json` / `appsettings.Development.json`
- If email verification is enabled, configure SMTP settings (see `Services/SmtpEmailSender.cs`)

## Future Improvements
- Improve UI responsiveness + accessibility
- Add better admin audit logging
- Add automated tests (unit + integration)
- Improve booking conflict detection & edge cases

## Screenshots
*(Add screenshots or a short GIF here once UI is polished.)*

## Author
Lukas Regulla