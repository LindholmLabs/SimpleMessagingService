using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class User
{
    [Key]
    [MaxLength(100)]
    [MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid Key { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}