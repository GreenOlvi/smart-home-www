using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
