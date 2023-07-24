using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartAc.Domain;

public sealed class IdempotentRequestEntry : EntityBase
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; }

    [Required] public string FromCommand { get; set; } = null!;

    [Required] 
    public DateTimeOffset OccurenceDateTime { get; private set; } = DateTimeOffset.UtcNow;

    [Required] public string HashString { get; set; } = null!;
}