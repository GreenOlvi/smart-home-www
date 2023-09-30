using NSubstitute;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Controllers;
using SmartHomeWWW.Server.Repositories;

namespace SmartHomeWWW.Server.Tests.Controllers;

public sealed class WeatherControllerTests
{
    private readonly ILogger<WeatherController> _weatherLogger = NullLogger<WeatherController>.Instance;
    private readonly ILogger<WeatherReportRepository> _weatherRepoLogger = NullLogger<WeatherReportRepository>.Instance;
    private readonly IMessageBus _messageBusMock = Substitute.For<IMessageBus>();

    [Test]
    public async Task GetCurrentWeatherShouldNotCrashWithNoDataTestAsync()
    {
        var cf = CreateContextFactory();
        using var db = cf.CreateDbContext();
        var repo = new WeatherReportRepository(_weatherRepoLogger, db);

        var controller = new WeatherController(_weatherLogger, CreateContextFactory(), _messageBusMock, db, repo);
        (await controller.GetCurrent()).Should().BeOfType<NotFoundResult>();
    }

    [Test]
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

        var cf = CreateContextFactory();

        using var context = cf.CreateDbContext();
        context!.WeatherCaches.Add(new WeatherCache
        {
            Id = Guid.NewGuid(),
            Name = "current",
            Data = serialized,
            Timestamp = timestamp.AddSeconds(1),
            Expires = timestamp.AddDays(1),
        });
        await context.SaveChangesAsync();

        var repo = new WeatherReportRepository(_weatherRepoLogger, context);

        var controller = new WeatherController(_weatherLogger, cf, _messageBusMock, context, repo);

        var result = await controller.GetCurrent();
        result.Should().BeOfType<OkObjectResult>();
        var okObject = (OkObjectResult)result;
        okObject.Value.Should().BeEquivalentTo(weather);
    }

    [Test]
    public async Task PostNewWeatherDataTestAsync()
    {
        var cf = CreateContextFactory();
        using var context = cf.CreateDbContext();
        var repo = new WeatherReportRepository(_weatherRepoLogger, context);

        var controller = new WeatherController(_weatherLogger, cf, _messageBusMock, context, repo);

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
        response.Should().BeOfType<OkResult>();
    }
}
