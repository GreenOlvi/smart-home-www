using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.ViewModel;
using SmartHomeWWW.Server.Messages;

namespace SmartHomeWWW.Server.Controllers
{
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RelayEntryViewModel>>> GetRelays()
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var relays = await dbContext.Relays
                .Select(r => RelayEntryViewModel.From(r))
                .ToArrayAsync();
            return Ok(relays);
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

        public record struct RelayData
        {
            public string Type { get; init; }
            public string Name { get; init; }
            public object Config { get; init; }
        }

        [HttpPost]
        public async Task<ActionResult> AddRelay([FromBody] RelayData relayInput)
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
            _logger.LogInformation("Deleted relay {id}", relay.Id);
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

            var relay = _relayFactory.Create(relayEntry);
            var status = await relay.GetStateAsync();

            return new RelayStateViewModel
            {
                RelayId = id,
                State = status.HasValue ? status.Value : null,
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

            var relay = _relayFactory.Create(relayEntry);

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
                State = state.HasValue ? state.Value : null,
            });
        }
    }
}
