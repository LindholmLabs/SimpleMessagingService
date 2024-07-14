namespace Shared.Contracts;

public class NewUserContract
{
    public string Name { get; set; } = string.Empty;
    public Guid Key { get; set; } = Guid.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}