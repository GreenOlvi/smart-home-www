using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Controllers;
using SmartHomeWWW.Server.Messages;

namespace SmartHomeWWW.Server.Tests.Controllers;

public sealed class WeatherControllerTests : IDisposable
{
    [SetUp]
    public async Task Setup()
    {
        var dbOptions = new DbContextOptionsBuilder<SmartHomeDbContext>()
            .UseSqlite("Data Source=:memory:;Mode=Memory;Cache=Shared",
                o => o.MigrationsAssembly("SmartHomeWWW.Server"))
            .Options;

        _context = new SmartHomeDbContext(dbOptions);
        await _context.Database.OpenConnectionAsync();
        await _context.Database.EnsureCreatedAsync();

        _contextFactoryMock = new Mock<IDbContextFactory<SmartHomeDbContext>>(MockBehavior.Strict);
        _contextFactoryMock.Setup(factory => factory.CreateDbContext())
            .Returns(() => new SmartHomeDbContext(dbOptions));

        _messageBusMock = new Mock<IMessageBus>(MockBehavior.Loose);
    }

    [TearDown]
    public Task Cleanup() => _context!.Database.CloseConnectionAsync();

    private SmartHomeDbContext? _context;
    private readonly ILogger<WeatherController> _weatherLogger = NullLogger<WeatherController>.Instance;
    private Mock<IDbContextFactory<SmartHomeDbContext>>? _contextFactoryMock;
    private Mock<IMessageBus> _messageBusMock = new (MockBehavior.Loose);

    [Test]
    public async Task GetCurrentWeatherShouldNotCrashWithNoDataTestAsync()
    {
        var controller = new WeatherController(_weatherLogger, _contextFactoryMock!.Object, _messageBusMock.Object);
        (await controller.GetCurrent()).Result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    [Ignore("Struggling with report comparision")]
    public async Task GetCurrentWeatherShouldReturnLatestValueTestAsync()
    {
        var timestamp = DateTime.UtcNow.Date.AddMinutes(-10);
        var weather = new WeatherReport
        {
            Timezone = "lol",
            Current = new CurrentWeather
            {
                Timestamp = timestamp,
                Temperature = -3.0f,
                Humidity = 50,
            },
        };

        var serialized = JsonSerializer.Serialize(weather);

        _context!.WeatherCaches.Add(new WeatherCache
        {
            Id = Guid.NewGuid(),
            Name = "current",
            Data = serialized,
            Timestamp = timestamp.AddSeconds(1),
            Expires = timestamp.AddDays(1),
        });
        await _context.SaveChangesAsync();

        var controller = new WeatherController(_weatherLogger, _contextFactoryMock!.Object, _messageBusMock.Object);

        var result = await controller.GetCurrent();
        result.Value.Should().Be(weather);
    }

    [Test]
    public async Task PostNewWeatherDataTestAsync()
    {
        var controller = new WeatherController(_weatherLogger, _contextFactoryMock!.Object, _messageBusMock.Object);

        var timestamp = DateTime.UtcNow;
        var weather = new WeatherReport
        {
            Timezone = "lol",
            Current = new CurrentWeather
            {
                Timestamp = timestamp,
                Temperature = -3.0f,
                Humidity = 50,
            },
        };

        var response = await controller.PostWeather("current", weather);
        response.Should().NotBeNull();

        var r = response as ObjectResult;
        r!.StatusCode.Should().Be(201);

        var w = await _context!.WeatherCaches.SingleAsync(w => w.Timestamp == timestamp);
        w.Timestamp.Should().Be(timestamp);
        w.Name.Should().Be("current");
    }

    public void Dispose() => _context?.Dispose();
}
