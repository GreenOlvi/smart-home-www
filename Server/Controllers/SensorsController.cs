using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : ControllerBase
    {
        public SensorsController(ILogger<SensorsController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        private readonly ILogger<SensorsController> _logger;
        private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetSensors()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Sensors.ToListAsync();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSensor(Guid id)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var sensor = await db.Sensors.FindAsync(id);

            if (sensor == null)
            {
                return NotFound();
            }

            db.Sensors.Remove(sensor);
            await db.SaveChangesAsync();
            _logger.LogInformation("Deleted {sensor}.", sensor);
            return Ok();
        }
    }
}
