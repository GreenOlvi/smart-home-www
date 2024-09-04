using NUnit.Framework;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using System.Reflection;
using System.Text.Json;

namespace SmartHomeWWW.Core.Tests.OpenWeatherMaps;

[TestFixture]
public class WeatherReportTests
{
    [Test]
    public async Task ParsingOneCall30Test()
    {
        var resourceName = "SmartHomeWWW.Core.Tests.OpenWeatherMaps.OneCall30.json";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        var report = await JsonSerializer.DeserializeAsync<WeatherReport>(stream!);
    }
}
