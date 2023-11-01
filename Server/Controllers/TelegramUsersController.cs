using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.ViewModel;

namespace SmartHomeWWW.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TelegramUsersController(ILogger<TelegramUsersController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory) : ControllerBase
{
    private readonly ILogger<TelegramUsersController> _logger = logger;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory = dbContextFactory;

    [HttpGet]
    public async Task<IEnumerable<TelegramUserViewModel>> Get()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return (await context.TelegramUsers.ToArrayAsync())
            .Select(TelegramUserViewModel.From);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TelegramUserViewModel>> Get(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = await context.TelegramUsers.FindAsync(id);
        return user is not null ? Ok(user) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] TelegramUserViewModel user)
    {
        if (user.Id is not null || user.TelegramId is null || user.Username is null)
        {
            return new StatusCodeResult(422);
        }

        using var context = _dbContextFactory.CreateDbContext();
        var newUser = await context.TelegramUsers.AddAsync(new TelegramUser
        {
            Id = Guid.NewGuid(),
            TelegramId = user.TelegramId.Value,
            Username = user.Username,
            UserType = user.UserType ?? string.Empty,
        });
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = newUser.Entity.Id }, TelegramUserViewModel.From(newUser.Entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put(Guid id, [FromBody] TelegramUserViewModel changed)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = await context.TelegramUsers.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var change = false;

        if (changed.TelegramId is not null && changed.TelegramId != user.TelegramId)
        {
            user.TelegramId = changed.TelegramId.Value;
            change = true;
        }

        if (changed.Username is not null && changed.Username != user.Username)
        {
            user.Username = changed.Username;
            change = true;
        }

        if (changed.UserType is not null && changed.UserType != user.UserType)
        {
            user.UserType = changed.UserType;
            change = true;
        }

        if (change)
        {
            context.TelegramUsers.Update(user);
            await context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = await context.TelegramUsers.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        context.TelegramUsers.Remove(user);
        await context.SaveChangesAsync();
        return Ok();
    }
}
