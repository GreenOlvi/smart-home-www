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
        public string Type { get; set; }
        public string Name { get; set; }

        [Column("Config")]
        public string ConfigSerialized
        {
            get => JsonSerializer.Serialize(Config);
            set => Config = JsonSerializer.Deserialize<object>(value);
        }

        [NotMapped]
        public object Config { get; set; }
    }
}
