using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SmartHomeWWW.Core.Domain.Entities
{
    [Table("Relays")]
    public class RelayEntry
    {
        [Key]
        public Guid Id { get; init; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        [Column("Config")]
        public string ConfigSerialized
        {
            get => JsonSerializer.Serialize(Config);
            set => Config = JsonSerializer.Deserialize<object>(value) ?? new { };
        }

        [NotMapped]
        public object Config { get; set; } = new { };
    }
}
