namespace Shared.Contracts;

public class GetMessagesContract
{
    public DateTime createdAt { get; set; } = DateTime.Now;
    public string createdBy { get; set; } = Environment.MachineName;
    public string origin { get; set; } = string.Empty;
    public List<MessageData> messages { get; set; } = new List<MessageData>();
}

public class MessageData
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Sender { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public string Content { get; set; } = string.Empty;
}