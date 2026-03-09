using BeanScene.Web.Data;
using BeanScene.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BeanScene.Web.Services;

public class ReservationValidator : IReservationValidator
{
    private readonly BeanSceneContext _context;

    public ReservationValidator(BeanSceneContext context)
    {
        _context = context;
    }

    public Task<(SittingSchedule? Sitting, ValidationResult Result)> ValidateCreateAsync(int sittingId, int guests, DateTime startTime)
        => ValidateAsync(sittingId, guests, startTime, excludeReservationId: null);

    public Task<(SittingSchedule? Sitting, ValidationResult Result)> ValidateEditAsync(int sittingId, int guests, int reservationId, DateTime startTime)
        => ValidateAsync(sittingId, guests, startTime, excludeReservationId: reservationId);

    private async Task<(SittingSchedule? Sitting, ValidationResult Result)> ValidateAsync(
        int sittingId, int guests, DateTime startTime, int? excludeReservationId)
    {
        var result = new ValidationResult();

        var sitting = await _context.SittingSchedules.FindAsync(sittingId);
        if (sitting == null)
        {
            result.Add("SittingId", "Invalid sitting selected.");
            return (null, result);
        }

        if (sitting.Status == SittingStatus.Closed)
            result.Add("SittingId", "This sitting is CLOSED. No reservations allowed.");

        if (startTime < sitting.StartDateTime || startTime > sitting.EndDateTime)
            result.Add("StartTime",
                $"Start time must be between {sitting.StartDateTime:h:mm tt} and {sitting.EndDateTime:h:mm tt}.");

        // Exclude the given reservation from the booked count (used on edits so the
        // reservation doesn't count against its own sitting capacity).
        var alreadyBooked = await _context.Reservations
            .Where(r => r.SittingId == sittingId &&
                        (excludeReservationId == null || r.ReservationId != excludeReservationId))
            .SumAsync(r => (int?)r.NumOfGuests) ?? 0;

        if (alreadyBooked + guests > sitting.Scapacity)
            result.Add("NumOfGuests", "Sitting capacity exceeded.");

        return (sitting, result);
    }
}
