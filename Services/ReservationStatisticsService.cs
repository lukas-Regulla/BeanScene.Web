using BeanScene.Web.Data;
using BeanScene.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BeanScene.Web.Services;

public class ReservationStatisticsService : IReservationStatisticsService
{
    private readonly BeanSceneContext _context;

    public ReservationStatisticsService(BeanSceneContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetTodayStatsAsync()
    {
        var today = DateTime.Today;
        var now = DateTime.Now;

        // Load today's reservations once; small set so in-memory grouping is fine.
        var todayRes = await _context.Reservations
            .Where(r => r.StartTime.Date == today)
            .Include(r => r.Sitting)
            .ToListAsync();

        // 7-day trend: past 6 days + today. Fetch only the StartTime column.
        var weekStart = today.AddDays(-6);
        var weekEnd = today.AddDays(1);
        var weekTimes = await _context.Reservations
            .Where(r => r.StartTime >= weekStart && r.StartTime < weekEnd)
            .Select(r => r.StartTime)
            .ToListAsync();

        var trendDict = weekTimes
            .GroupBy(dt => dt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var weekTrend = Enumerable.Range(0, 7)
            .Select(i => today.AddDays(i - 6))
            .Select(d => new DayStatDto
            {
                Label = d.ToString("d MMM"),
                Total = trendDict.GetValueOrDefault(d, 0)
            })
            .ToList();

        // Next 8 non-cancelled reservations from now.
        var upcoming = await _context.Reservations
            .Where(r => r.StartTime >= now && r.Status != "Cancelled")
            .OrderBy(r => r.StartTime)
            .Take(8)
            .Select(r => new UpcomingReservationDto
            {
                ReservationId = r.ReservationId,
                GuestName = r.FirstName + " " + r.LastName,
                StartTime = r.StartTime,
                NumOfGuests = r.NumOfGuests,
                Status = r.Status,
                SittingName = r.Sitting != null ? r.Sitting.Stype : ""
            })
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalToday = todayRes.Count,
            PendingToday = todayRes.Count(r => r.Status == "Pending"),
            ConfirmedToday = todayRes.Count(r => r.Status == "Confirmed"),
            CancelledToday = todayRes.Count(r => r.Status == "Cancelled"),
            OverallPending = await _context.Reservations.CountAsync(r => r.Status == "Pending"),
            OverallConfirmed = await _context.Reservations.CountAsync(r => r.Status == "Confirmed"),
            TodayBySitting = todayRes
                .GroupBy(r => r.Sitting?.Stype ?? "Unknown")
                .Select(g => new SittingStatDto { SittingName = g.Key, Count = g.Count() })
                .OrderBy(s => s.SittingName)
                .ToList(),
            UpcomingReservations = upcoming,
            WeekTrend = weekTrend,
        };
    }
}
