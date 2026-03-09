namespace BeanScene.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Today's counts
        public int TotalToday { get; set; }
        public int PendingToday { get; set; }
        public int ConfirmedToday { get; set; }
        public int CancelledToday { get; set; }

        // Overall pipeline
        public int OverallPending { get; set; }
        public int OverallConfirmed { get; set; }

        // Charts / detail panels
        public List<SittingStatDto> TodayBySitting { get; set; } = new();
        public List<UpcomingReservationDto> UpcomingReservations { get; set; } = new();
        public List<DayStatDto> WeekTrend { get; set; } = new();
    }

    public class SittingStatDto
    {
        public string SittingName { get; set; } = "";
        public int Count { get; set; }
    }

    public class UpcomingReservationDto
    {
        public int ReservationId { get; set; }
        public string GuestName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public int NumOfGuests { get; set; }
        public ReservationStatus Status { get; set; }
        public string SittingName { get; set; } = "";
    }

    public class DayStatDto
    {
        public string Label { get; set; } = "";
        public int Total { get; set; }
    }
}
