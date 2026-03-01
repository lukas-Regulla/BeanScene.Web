using BeanScene.Web.Models;

namespace BeanScene.Web.Services;

public interface IReservationValidator
{
    Task<(SittingSchedule? Sitting, ValidationResult Result)> ValidateCreateAsync(int sittingId, int guests);
    Task<(SittingSchedule? Sitting, ValidationResult Result)> ValidateEditAsync(int sittingId, int guests, int reservationId);
}
