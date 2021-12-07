using NUnit.Framework;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SmartHomeWWW.Controllers;
using Microsoft.EntityFrameworkCore;
using SmartHomeCore.Infrastructure;
using System.Threading.Tasks;
using System;

namespace SmartHomeWWWTests
{
    public class WeatherControllerTests
    {
        [SetUp]
        public async Task Setup()
        {
            var dbOptions = new DbContextOptionsBuilder<SmartHomeDbContext>()
                .UseSqlite("Data Source=SmartHomeDb;Mode=Memory;Cache=Shared",
                    o => o.MigrationsAssembly("SmartHomeWWW"))
                .Options;

            _context = new SmartHomeDbContext(dbOptions);
            await _context.Database.OpenConnectionAsync();
            await _context.Database.EnsureCreatedAsync();

            _contextFactoryMock = new Mock<IDbContextFactory<SmartHomeDbContext>>(MockBehavior.Strict);
            _contextFactoryMock.Setup(factory => factory.CreateDbContext())
                .Returns(() => new SmartHomeDbContext(dbOptions));

            _weatherLogger = NullLogger<WeatherController>.Instance;
        }

        [TearDown]
        public async Task Cleanup()
        {
            await _context.Database.CloseConnectionAsync();
        }

        private SmartHomeDbContext _context;
        private ILogger<WeatherController> _weatherLogger;
        private Mock<IDbContextFactory<SmartHomeDbContext>> _contextFactoryMock;

        [Test]
        public async Task GetCurrentWeatherShouldNotCrashWithNoDataTestAsync()
        {
            var controller = new WeatherController(_weatherLogger, _contextFactoryMock.Object);

            var weather = await controller.GetCurrent();

            weather.Should().BeNull();
        }

        [Test]
        public async Task GetCurrentWeatherShouldReturnLatestValueTestAsync()
        {
            _context.WeatherCaches.Add(new SmartHomeCore.Domain.WeatherCache
            {
                Id = Guid.NewGuid(),
                Name = "current",
                Data = @"{""hello"":""world""}",
                Timestamp = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1),
            });
            await _context.SaveChangesAsync();

            var controller = new WeatherController(_weatherLogger, _contextFactoryMock.Object);

            var weather = await controller.GetCurrent();
            weather.Should().Be(@"{""hello"":""world""}");
        }
    }
}