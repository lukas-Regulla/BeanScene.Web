namespace BeanScene.Web.Services;

public class ValidationResult
{
    public Dictionary<string, string> Errors { get; } = new();
    public bool IsValid => Errors.Count == 0;
    public void Add(string key, string message) => Errors[key] = message;
}
