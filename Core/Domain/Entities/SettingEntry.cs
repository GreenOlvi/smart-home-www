using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeWWW.Core.Domain.Entities;

[Index(nameof(Key), IsUnique = true)]
public record SettingEntry
{
    public Guid Id { get; init; }
    [Required]
    public string Key { get; init; } = null!;
    public string Value { get; init; } = null!;
}
