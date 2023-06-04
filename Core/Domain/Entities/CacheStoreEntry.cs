using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SmartHomeWWW.Core.Domain.Entities;

[Index(nameof(Key), IsUnique = true)]
[Index(nameof(ExpireAt), IsUnique = false)]
public class CacheStoreEntry
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DateTime? ExpireAt { get; set; }
}
