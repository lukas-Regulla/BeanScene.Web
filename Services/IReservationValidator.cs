using BeanScene.Web.Models;

namespace BeanScene.Web.Services;

public interface IReservationValidator
{
    Task<(SittingSchedule? Sitting, ValidationResult Result)> ValidateCreateAsync(int sittingId, int guests, DateTime startTime);
    Task<(SittingSchedule? Sitting, ValidationResult Result)> ValidateEditAsync(int sittingId, int guests, int reservationId, DateTime startTime);
}
