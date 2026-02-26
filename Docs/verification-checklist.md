# Manual Verification Checklist

Run before and after refactors:
- dotnet build
- dotnet run

## Identity / Roles
- Admin can access AdminController
- Staff can manage reservations
- Member can only view/cancel their own reservations
- Email confirmation requirement still enforced

## Reservation Creation
- Can create reservation for OPEN sitting
- Cannot create reservation for CLOSED sitting
- Capacity validation still works

## Assign Tables
- Prevent double booking of tables
- Total table seats >= guest count required
- Status changes Pending → Confirmed