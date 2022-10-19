using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Controllers;
using SmartHomeWWW.Server.Hubs;
using System.Collections;
using System.Text;

namespace SmartHomeWWW.Server.Tests.Controllers;

[TestFixture]
public class UpdateControllerTests
{
    private IServiceProvider _sp = new Mock<IServiceProvider>(MockBehavior.Strict).Object;
    private SmartHomeDbContext? _db;

    [SetUp]
    public void Setup()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ILogger<UpdateController>>(sp => NullLogger<UpdateController>.Instance);

        var dbName = Guid.NewGuid().ToString();
        var contextFactoryMock = new Mock<IDbContextFactory<SmartHomeDbContext>>(MockBehavior.Strict);
        contextFactoryMock.Setup(factory => factory.CreateDbContext())
            .Returns(() => SmartHomeDbTestContextFactory.CreateInMemoryAsync(dbName).Result);
        sc.AddSingleton(sp => contextFactoryMock.Object);

        sc.AddTransient(sp => SmartHomeDbTestContextFactory.CreateInMemoryAsync(dbName).Result);

        var hubConn = new Mock<IHubConnection>(MockBehavior.Strict);
        hubConn.Setup(h => h.State).Returns(HubConnectionState.Connected);
        hubConn.Setup(h => h.SendAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        sc.AddTransient(sp => hubConn.Object);

        _sp = sc.BuildServiceProvider();
        _db = _sp.GetRequiredService<SmartHomeDbContext>();
    }

    [TearDown]
    public void Teardown()
    {
        _db?.Dispose();
    }

    [Test]
    public async Task UpdateFirmwareDefaultQueryTest()
    {
        var repo = new Mock<IFirmwareRepository>(MockBehavior.Strict);

        var context = new DefaultHttpContext();
        var controller = new UpdateController(
            _sp.GetRequiredService<ILogger<UpdateController>>(),
            _sp.GetRequiredService<IHubConnection>(),
            repo.Object,
            _sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>())
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = context,
            },
        };

        var result = await controller.Firmware();
        result.Should().BeOfType<RedirectResult>();
    }

    [Test]
    public async Task UpdateFirmwareAsNewEspSensorCurrentFirmwareTest()
    {
        var repo = new MemoryFirmwareRepository
        {
            new MemoryFirmware(new Version("0.0.9")),
            new MemoryFirmware(new Version("0.9.0")),
            new MemoryFirmware(new Version("1.0.0")),
        };

        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "ESP8266-http-Update";
        context.Request.Headers["x-ESP8266-STA-MAC"] = "DE:AD:BE:EF:00:01";
        context.Request.Headers["x-ESP8266-version"] = "1.0.0";

        var controller = new UpdateController(
            _sp.GetRequiredService<ILogger<UpdateController>>(),
            _sp.GetRequiredService<IHubConnection>(),
            repo,
            _sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>())
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = context,
            },
        };

        var result = await controller.Firmware();
        result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result).StatusCode.Should().Be(304);

        var sensors = await _db!.Sensors.ToArrayAsync();
        sensors.Should().HaveCount(1);

        var sensor = sensors[0];
        sensor.ChipType.Should().Be("ESP8266");
        sensor.Mac.Should().Be("DE:AD:BE:EF:00:01");
        sensor.FirmwareVersion.Should().Be("1.0.0");
        sensor.UpdateChannel.Should().BeNull();
    }

    [Test]
    public async Task UpdateFirmwareAsNewEspSensorOldFirmwareTest()
    {
        var repo = new MemoryFirmwareRepository
        {
            new MemoryFirmware(new Version("0.0.9")),
            new MemoryFirmware(new Version("0.9.0")),
            new MemoryFirmware(new Version("1.0.0")),
        };

        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "ESP8266-http-Update";
        context.Request.Headers["x-ESP8266-STA-MAC"] = "DE:AD:BE:EF:00:01";
        context.Request.Headers["x-ESP8266-version"] = "0.7.0";

        var controller = new UpdateController(
            _sp.GetRequiredService<ILogger<UpdateController>>(),
            _sp.GetRequiredService<IHubConnection>(),
            repo,
            _sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>())
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = context,
            },
        };

        var result = await controller.Firmware();
        result.Should().BeOfType<FileStreamResult>();
        ((FileStreamResult)result).FileDownloadName.Should().Be("firmware.1.0.0.bin");

        var sensors = await _db!.Sensors.ToArrayAsync();

        sensors.Should().HaveCount(1);

        var sensor = sensors[0];
        sensor.ChipType.Should().Be("ESP8266");
        sensor.Mac.Should().Be("DE:AD:BE:EF:00:01");
        sensor.FirmwareVersion.Should().Be("0.7.0");
        sensor.UpdateChannel.Should().BeNull();
    }

    [Test]
    public async Task UpdateFirmwareAsNewEspSensorNewerFirmwareTest()
    {
        var repo = new MemoryFirmwareRepository
        {
            new MemoryFirmware(new Version("0.0.9")),
            new MemoryFirmware(new Version("0.9.0")),
            new MemoryFirmware(new Version("1.0.0")),
        };

        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "ESP8266-http-Update";
        context.Request.Headers["x-ESP8266-STA-MAC"] = "DE:AD:BE:EF:00:01";
        context.Request.Headers["x-ESP8266-version"] = "1.0.1";

        var controller = new UpdateController(
            _sp.GetRequiredService<ILogger<UpdateController>>(),
            _sp.GetRequiredService<IHubConnection>(),
            repo,
            _sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>())
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = context,
            },
        };

        var result = await controller.Firmware();
        result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result).StatusCode.Should().Be(304);

        var sensors = await _db!.Sensors.ToArrayAsync();
        sensors.Should().HaveCount(1);

        var sensor = sensors[0];
        sensor.ChipType.Should().Be("ESP8266");
        sensor.Mac.Should().Be("DE:AD:BE:EF:00:01");
        sensor.FirmwareVersion.Should().Be("1.0.1");
        sensor.UpdateChannel.Should().BeNull();
    }

    [Test]
    public async Task UpdateFirmwareChannelSensorOldFirmwareTest()
    {
        _db!.Sensors.Add(new Core.Domain.Entities.Sensor
        {
            Id = Guid.NewGuid(),
            Alias = "Alpha-01",
            ChipType = "ESP8266",
            FirmwareVersion = "0.7.0-alpha",
            UpdateChannel = "alpha",
            Mac = "DE:AD:BE:EF:00:01",
        });
        await _db.SaveChangesAsync();

        var repo = new MemoryFirmwareRepository
        {
            new MemoryFirmware(FirmwareVersion.Parse("0.9.0-alpha"), UpdateChannel.Alpha),
            new MemoryFirmware(FirmwareVersion.Parse("1.0.0-alpha"), UpdateChannel.Alpha),
            new MemoryFirmware(new Version("0.9.0")),
            new MemoryFirmware(new Version("1.0.0")),
            new MemoryFirmware(new Version("1.0.1")),
        };

        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "ESP8266-http-Update";
        context.Request.Headers["x-ESP8266-STA-MAC"] = "DE:AD:BE:EF:00:01";
        context.Request.Headers["x-ESP8266-version"] = "0.7.0-alpha";

        var controller = new UpdateController(
            _sp.GetRequiredService<ILogger<UpdateController>>(),
            _sp.GetRequiredService<IHubConnection>(),
            repo,
            _sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>())
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = context,
            },
        };

        var result = await controller.Firmware();
        result.Should().BeOfType<FileStreamResult>();
        ((FileStreamResult)result).FileDownloadName.Should().Be("firmware.1.0.0-alpha.bin");

        var sensors = await _db.Sensors.ToArrayAsync();

        sensors.Should().HaveCount(1);

        var sensor = sensors[0];
        sensor.Alias.Should().Be("Alpha-01");
        sensor.ChipType.Should().Be("ESP8266");
        sensor.Mac.Should().Be("DE:AD:BE:EF:00:01");
        sensor.FirmwareVersion.Should().Be("0.7.0-alpha"); // Will be updated on next contact
        sensor.UpdateChannel.Should().Be("alpha");
    }

    [Test]
    public async Task UpdateFirmwareChannelSensorNewerFirmwareTest()
    {
        var repo = new MemoryFirmwareRepository
        {
            new MemoryFirmware(new Version("0.0.9")),
            new MemoryFirmware(new Version("0.9.0")),
            new MemoryFirmware(FirmwareVersion.Parse("1.0.0-alpha")),
        };

        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "ESP8266-http-Update";
        context.Request.Headers["x-ESP8266-STA-MAC"] = "DE:AD:BE:EF:00:01";
        context.Request.Headers["x-ESP8266-version"] = "1.0.1-alpha";

        var controller = new UpdateController(
            _sp.GetRequiredService<ILogger<UpdateController>>(),
            _sp.GetRequiredService<IHubConnection>(),
            repo,
            _sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>())
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = context,
            },
        };

        var result = await controller.Firmware();
        result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result).StatusCode.Should().Be(304);

        var sensors = await _db!.Sensors.ToArrayAsync();
        sensors.Should().HaveCount(1);

        var sensor = sensors[0];
        sensor.ChipType.Should().Be("ESP8266");
        sensor.Mac.Should().Be("DE:AD:BE:EF:00:01");
        sensor.FirmwareVersion.Should().Be("1.0.1-alpha");
        sensor.UpdateChannel.Should().BeNull();
    }

    [Test]
    public async Task UpdateFirmwareChannelSensorInvalidFirmwareTest()
    {
        var repo = new MemoryFirmwareRepository
        {
            new MemoryFirmware(new Version("0.0.9")),
            new MemoryFirmware(new Version("0.9.0")),
            new MemoryFirmware(FirmwareVersion.Parse("1.0.0-alpha")),
        };

        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "ESP8266-http-Update";
        context.Request.Headers["x-ESP8266-STA-MAC"] = "DE:AD:BE:EF:00:01";
        context.Request.Headers["x-ESP8266-version"] = "some-nonsense";

        var controller = new UpdateController(
            _sp.GetRequiredService<ILogger<UpdateController>>(),
            _sp.GetRequiredService<IHubConnection>(),
            repo,
            _sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>())
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = context,
            },
        };

        var result = await controller.Firmware();
        result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result).StatusCode.Should().Be(304);

        var sensors = await _db!.Sensors.ToArrayAsync();
        sensors.Should().HaveCount(1);

        var sensor = sensors[0];
        sensor.ChipType.Should().Be("ESP8266");
        sensor.Mac.Should().Be("DE:AD:BE:EF:00:01");
        sensor.FirmwareVersion.Should().Be("some-nonsense");
        sensor.UpdateChannel.Should().BeNull();
    }

    private sealed class MemoryFirmwareRepository : IFirmwareRepository, IEnumerable<IFirmware>
    {
        private readonly List<IFirmware> _firmwares = new();

        public MemoryFirmwareRepository()
        {
        }

        public MemoryFirmwareRepository(IEnumerable<IFirmware> firmwares)
        {
            _firmwares = firmwares.ToList();
        }

        public IEnumerable<IFirmware> GetAllFirmwares() => _firmwares;

        public void Add(IFirmware firmware) => _firmwares.Add(firmware);
        public IEnumerator<IFirmware> GetEnumerator() => _firmwares.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _firmwares.GetEnumerator();
    }

    private sealed record MemoryFirmware : IFirmware
    {
        public MemoryFirmware()
        {
        }

        public MemoryFirmware(Version version)
        {
            Version = new FirmwareVersion { Prefix = version };
        }

        public MemoryFirmware(FirmwareVersion version, UpdateChannel channel = UpdateChannel.Stable)
        {
            Version = version;
            Channel = channel;
        }

        public FirmwareVersion Version { get; init; } = new();

        public long Size => Content.Length;

        public UpdateChannel Channel { get; init; } = UpdateChannel.Stable;

        public string Content { get; init; } = string.Empty;

        public Stream GetData() => new MemoryStream(Encoding.UTF8.GetBytes(Content));
    }
}
