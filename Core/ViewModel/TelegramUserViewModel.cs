using SmartHomeWWW.Core.Domain.Entities;

namespace SmartHomeWWW.Core.ViewModel;

public record TelegramUserViewModel
{
    public Guid? Id { get; init; }
    public long? TelegramId { get; set; }
    public string? Username { get; set; }
    public string? UserType { get; set; }

    public static TelegramUserViewModel From(TelegramUser user) => new ()
    {
        Id = user.Id,
        TelegramId = user.TelegramId,
        Username = user.Username,
        UserType = user.UserType,
    };

    public static implicit operator TelegramUserViewModel(TelegramUser user) => From(user);
}
