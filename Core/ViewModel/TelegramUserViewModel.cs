using SmartHomeWWW.Core.Domain.Entities;

namespace SmartHomeWWW.Core.ViewModel;

public readonly record struct TelegramUserViewModel
{
    public Guid? Id { get; init; }
    public long? TelegramId { get; init; }
    public string? Username { get; init; }
    public string? UserType { get; init; }

    public static TelegramUserViewModel From(TelegramUser user) => new()
    {
        Id = user.Id,
        TelegramId = user.TelegramId,
        Username = user.Username,
        UserType = user.UserType,
    };

    public static implicit operator TelegramUserViewModel(TelegramUser user) => From(user);
}
