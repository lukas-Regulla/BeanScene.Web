using BeanScene.Web.Data;
using BeanScene.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BeanScene.Web.Services;

public class TableAssignmentService : ITableAssignmentService
{
    private readonly BeanSceneContext _context;

    public TableAssignmentService(BeanSceneContext context)
    {
        _context = context;
    }

    public async Task<TableAssignmentResult> AssignAsync(Reservation reservation, List<int> selectedTableIds)
    {
        if (await HasTableConflictsAsync(reservation, selectedTableIds))
            return TableAssignmentResult.Failure(
                "One or more of these selected tables are already booked for this sitting.");

        int totalSeats = await GetTotalSeatsAsync(selectedTableIds);
        if (totalSeats < reservation.NumOfGuests)
            return TableAssignmentResult.Failure(
                $"Selected tables only seat {totalSeats}, but reservation requires {reservation.NumOfGuests}.");

        // Replace all existing table assignments for this reservation.
        var oldAssignments = _context.ReservationTables.Where(rt => rt.ReservationId == reservation.ReservationId);
        _context.ReservationTables.RemoveRange(oldAssignments);

        foreach (var tableId in selectedTableIds)
        {
            _context.ReservationTables.Add(new ReservationTable
            {
                ReservationId = reservation.ReservationId,
                RestaurantTableID = tableId
            });
        }

        // Assigning tables moves the reservation to Confirmed status.
        reservation.Status = "Confirmed";
        await _context.SaveChangesAsync();

        return TableAssignmentResult.Success();
    }

    // Double-booking check: reject any table already assigned to another reservation in this sitting.
    private async Task<bool> HasTableConflictsAsync(Reservation reservation, List<int> selectedTableIds)
    {
        return await _context.ReservationTables
            .Include(rt => rt.Reservation)
            .Where(rt => selectedTableIds.Contains(rt.RestaurantTableID)
                && rt.Reservation.SittingId == reservation.SittingId
                && rt.ReservationId != reservation.ReservationId)
            .AnyAsync();
    }

    private async Task<int> GetTotalSeatsAsync(List<int> selectedTableIds)
    {
        var tables = await _context.RestaurantTables
            .Where(t => selectedTableIds.Contains(t.RestaurantTableId))
            .ToListAsync();
        return tables.Sum(t => t.Seats ?? 0);
    }
}
