namespace Shared.Contracts;

public class GetUsersContract
{
    public DateTime createdAt { get; set; } = DateTime.Now;
    public string createdBy { get; set; } = Environment.MachineName;
    public string origin { get; set; } = string.Empty;
    public List<UserData> users { get; set; } = new List<UserData>();
}

public class UserData
{
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}