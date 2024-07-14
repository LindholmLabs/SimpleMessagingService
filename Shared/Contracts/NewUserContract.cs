namespace Shared.Contracts;

public class NewUserContract
{
    public string Name { get; set; } = string.Empty;
    public Guid Key { get; set; }
    public DateTime CreatedAt { get; set; }
}