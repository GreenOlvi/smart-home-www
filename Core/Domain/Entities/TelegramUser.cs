using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHomeWWW.Core.Domain.Entities
{
    [Table("TelegramUsers")]
    [Index(nameof(TelegramId), IsUnique = true)]
    public class TelegramUser
    {
        [Key]
        public Guid Id { get; init; }
        public long TelegramId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
    }
}
