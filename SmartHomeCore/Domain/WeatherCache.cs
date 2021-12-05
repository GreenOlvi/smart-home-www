using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHomeCore.Domain
{
    public class WeatherCache
    {
        [Key]
        public Guid Id { get; init; }
        public string Name { get; init; }
        public DateTime Timestamp { get; init; }
        public DateTime? Expires { get; init; }
        public string Data { get; init; }
    }
}
