using BeanScene.Web.Models;

namespace BeanScene.Web.Services;

public interface ITableAssignmentService
{
    Task<TableAssignmentResult> AssignAsync(Reservation reservation, List<int> selectedTableIds);
}

public class TableAssignmentResult
{
    public bool Succeeded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static TableAssignmentResult Success() =>
        new() { Succeeded = true };

    public static TableAssignmentResult Failure(string message) =>
        new() { Succeeded = false, ErrorMessage = message };
}
