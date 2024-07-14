namespace Shared.Contracts;

public class MessageContract
{
    public Guid Key { get; set; } = Guid.Empty;
    public string Content { get; set; } = string.Empty;
}