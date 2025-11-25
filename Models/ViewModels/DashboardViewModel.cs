namespace BeanScene.Web.Models.ViewModels

{
    public class DashboardViewModel
    {
        public int TotalToday { get; set; }
        public int PendingToday { get; set; }
        public int ConfirmedToday { get; set; }
        public int CancelledToday { get; set; }

        public int OverallPending { get; set; }
        public int OverallConfirmed { get; set; }
    }
}
