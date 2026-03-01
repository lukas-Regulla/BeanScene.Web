using BeanScene.Web.Models.ViewModels;

namespace BeanScene.Web.Services;

public interface IReservationStatisticsService
{
    Task<DashboardViewModel> GetTodayStatsAsync();
}
