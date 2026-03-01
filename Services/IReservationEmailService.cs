using BeanScene.Web.Models;

namespace BeanScene.Web.Services;

public interface IReservationEmailService
{
    Task SendCreatedAsync(Reservation reservation, SittingSchedule sitting);
    Task SendConfirmedAsync(Reservation reservation);
}
