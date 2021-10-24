﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Infrastructure;
using SmartHomeWWW.Models;
using System;
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

        private readonly ILogger<SensorsController> _logger;
        private readonly SmartHomeDbContext _dbContext;

        public async Task<IActionResult> Index()
        {
            var sensors = await _dbContext.Sensors.ToListAsync();
            return View(sensors.Select(SensorViewModel.FromSensor).ToArray());
        }

        [HttpDelete("/{controller}/Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var sensor = await _dbContext.Sensors.SingleAsync(s => s.Id == id);
            _dbContext.Sensors.Remove(sensor);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Deleted {sensor}.");
            return RedirectToAction("Index");
        }
    }
}