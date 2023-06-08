using Microsoft.AspNetCore.Http.HttpResults;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Server.Controllers;
using SmartHomeWWW.Server.Messages;

namespace SmartHomeWWW.Server.Tests.Controllers;

public sealed class WeatherControllerTests
{
    private readonly ILogger<WeatherController> _weatherLogger = NullLogger<WeatherController>.Instance;
    private readonly Mock<IMessageBus> _messageBusMock = new (MockBehavior.Loose);

    [Test]
    public async Task GetCurrentWeatherShouldNotCrashWithNoDataTestAsync()
    {
        var controller = new WeatherController(_weatherLogger, CreateContextFactory(), _messageBusMock.Object);
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

        var controller = new WeatherController(_weatherLogger, cf, _messageBusMock.Object);

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

        var controller = new WeatherController(_weatherLogger, cf, _messageBusMock.Object);

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

        var w = await context.WeatherCaches.SingleAsync(w => w.Timestamp == timestamp);
        w.Timestamp.Should().Be(timestamp);
        w.Name.Should().Be("current");
    }
}
