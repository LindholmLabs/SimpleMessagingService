using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime TimeStamp { get; set; } = DateTime.Now;

    [Required]
    [ForeignKey("User")]
    public string Name { get; set; } = String.Empty;

    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = String.Empty;
}