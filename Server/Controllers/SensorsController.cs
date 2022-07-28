using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Sensor>>> GetSensors()
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Sensors.ToListAsync();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        _logger.LogInformation("Deleted {Sensor}.", sensor);
        return Ok();
    }
}
