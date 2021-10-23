using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Infrastructure;
using SmartHomeWWW.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHomeWWW.Controllers
{
    public class SensorsController : Controller
    {
        public SensorsController(ILogger<SensorsController> logger, SmartHomeDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private ILogger<SensorsController> _logger;
        private SmartHomeDbContext _dbContext;

        public async Task<IActionResult> IndexAsync()
        {
            var sensors = await _dbContext.Sensors.ToListAsync();

            var list = new SensorListViewModel
            {
                Sensors = sensors.Select(SensorViewModel.FromSensor).ToArray(),
            };
            return View(list);
        }
    }
}
