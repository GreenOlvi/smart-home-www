using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SmartHomeWWW.Core.Domain.Entities;

[Index(nameof(Key), IsUnique = true)]
public record SettingEntry
{
    public Guid Id { get; init; }
    [Required]
    public string Key { get; init; } = null!;
    public string Value { get; init; } = null!;
}
