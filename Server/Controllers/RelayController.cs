using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.ViewModel;
using SmartHomeWWW.Server.Messages;
using System.Text.Json;

namespace SmartHomeWWW.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RelayController : ControllerBase
{
    public RelayController(ILogger<RelayController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory, IRelayFactory relayFactory, IMessageBus messageBus, IServiceProvider sp)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _relayFactory = relayFactory;
        _bus = messageBus;
        _sp = sp;
    }

    private readonly ILogger<RelayController> _logger;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;
    private readonly IRelayFactory _relayFactory;
    private readonly IMessageBus _bus;
    private readonly IServiceProvider _sp;

    public record RelayListParameters
    {
        public string? Type { get; init; }
        public TasmotaClientKind? Kind { get; init; }
        public string? Search { get; init; }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RelayEntryViewModel>>> GetRelays([FromQuery] RelayListParameters parameters)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var relays = await ApplyFilter(dbContext.Relays, parameters)
            .Select(r => RelayEntryViewModel.From(r))
            .ToArrayAsync();

        if (parameters.Kind is not null)
        {
            relays = relays.Where(r => r.Kind == parameters.Kind).ToArray();
        }

        return Ok(relays);
    }

    private static IQueryable<RelayEntry> ApplyFilter(IQueryable<RelayEntry> relays, RelayListParameters parameters)
    {
        var r = relays;
        if (parameters.Type is not null)
        {
            r = r.Where(x => x.Type == parameters.Type);
        }
        if (parameters.Search is not null)
        {
            r = r.Where(x => EF.Functions.Like(x.Name, "%" + parameters.Search + "%"));
        }
        return r;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RelayEntryViewModel>> GetRelay(Guid id)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var relay = await dbContext.Relays.FindAsync(id);

        if (relay is null)
        {
            return NotFound();
        }

        return RelayEntryViewModel.From(relay);
    }

    public readonly record struct NewRelayData
    {
        public string Type { get; init; }
        public string Name { get; init; }
        public object Config { get; init; }
    }

    [HttpPost]
    public async Task<ActionResult> AddRelay([FromBody] NewRelayData relayInput)
    {
        var relay = new RelayEntry
        {
            Id = Guid.NewGuid(),
            Type = relayInput.Type,
            Name = relayInput.Name,
            Config = relayInput.Config,
        };
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.Relays.Add(relay);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRelay), new { id = relay.Id }, relay);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRelay(Guid id)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var relay = await dbContext.Relays.FindAsync(id);

        if (relay is null)
        {
            return NotFound();
        }

        dbContext.Relays.Remove(relay);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted relay {Id}", relay.Id);
        return Ok();
    }

    [HttpGet("{id}/state")]
    public async Task<ActionResult<RelayStateViewModel>> GetState(Guid id)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var relayEntry = await dbContext.Relays.FindAsync(id);

        if (relayEntry is null)
        {
            return NotFound();
        }

        using var relay = _relayFactory.Create(relayEntry);
        var status = await relay.GetStateAsync();

        return new RelayStateViewModel
        {
            RelayId = id,
            State = status,
        };
    }

    [HttpPost("{id}/state")]
    public async Task<ActionResult> ExecuteCommand(Guid id, [FromForm] string value)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var relayEntry = await dbContext.Relays.FindAsync(id);

        if (relayEntry is null)
        {
            return NotFound();
        }

        using var relay = _relayFactory.Create(relayEntry);

        switch (value.ToLowerInvariant())
        {
            case "toggle":
                await relay.ToggleAsync();
                break;
            case "on":
                await relay.SetStateAsync(true);
                break;
            case "off":
                await relay.SetStateAsync(false);
                break;
            default:
                throw new InvalidOperationException($"Unknown action type '{value}'");
        }

        var state = await relay.GetStateAsync();

        return Ok(new RelayStateViewModel
        {
            RelayId = id,
            State = state,
        });
    }
}
