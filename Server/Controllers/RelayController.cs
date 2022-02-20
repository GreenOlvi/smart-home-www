using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;

namespace SmartHomeWWW.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelayController : ControllerBase
    {
        public RelayController(ILogger<RelayController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        private readonly ILogger<RelayController> _logger;
        private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RelayEntry>>> GetRelays()
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.Relays.ToArrayAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RelayEntry>> GetRelay(Guid id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var relay = await dbContext.Relays.FindAsync(id);

            if (relay is null)
            {
                return NotFound();
            }

            return relay;
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
    }
}
