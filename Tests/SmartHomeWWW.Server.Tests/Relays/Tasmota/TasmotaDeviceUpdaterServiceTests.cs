using FluentAssertions.Execution;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Relays.Tasmota;

namespace SmartHomeWWW.Server.Tests.Relays.Tasmota;

[TestFixture]
public class TasmotaDeviceUpdaterServiceTests
{
    private static readonly ILogger<TasmotaDeviceUpdaterService> Logger = NullLogger<TasmotaDeviceUpdaterService>.Instance;
    private static readonly TasmotaDiscoveryConfig Config = new() { UpdateHttpRelays = true, UpdateMqttRelays = true };

    private const string SmartPlug = """{"ip":"192.168.1.101","dn":"SmartPlug","fn":["SmartPlug",null,null,null,null,null,null,null],"hn":"tasmota-67890A-9876","mac":"01234567890A","md":"Neo Coolcam 16","ty":0,"if":0,"ofln":"Offline","onln":"Online","state":["OFF","ON","TOGGLE","HOLD"],"sw":"12.1.1","t":"tasmota_67890A","ft":"%prefix%/%topic%/","tp":["cmnd","stat","tele"],"rl":[1,0,0,0,0,0,0,0],"swc":[-1,-1,-1,-1,-1,-1,-1,-1],"swn":[null,null,null,null,null,null,null,null],"btn":[0,0,0,0,0,0,0,0],"so":{"4":0,"11":0,"13":0,"17":0,"20":0,"30":0,"68":0,"73":0,"82":0,"114":0,"117":0},"lk":0,"lt_st":0,"sho":[0,0,0,0],"sht":[[0,0,0],[0,0,0],[0,0,0],[0,0,0]],"ver":1}""";
    private const string LightSwitch = """{"ip":"192.168.1.102","dn":"TasmotaLightSwitch1","fn":["TasmotaLightSwitch1","",null,null,null,null,null,null],"hn":"tasmota-334455-6543","mac":"001122334455","md":"Sonoff T0 2CH","ty":0,"if":0,"ofln":"Offline","onln":"Online","state":["OFF","ON","TOGGLE","HOLD"],"sw":"12.4.0","t":"tasmota_334455","ft":"%prefix%/%topic%/","tp":["cmnd","stat","tele"],"rl":[1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],"swc":[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],"swn":[null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null],"btn":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],"so":{"4":0,"11":0,"13":0,"17":0,"20":0,"30":0,"68":0,"73":0,"82":0,"114":0,"117":0},"lk":0,"lt_st":0,"sho":[0,0,0,0],"sht":[[0,0,0],[0,0,0],[0,0,0],[0,0,0]],"ver":1}""";

    [Test]
    public async Task NewDeviceMessageShouldAddNewDeviceTest()
    {
        var config = JsonSerializer.Deserialize<TasmotaDiscoveryMessage>(SmartPlug)!;
        var cf = CreateContextFactory();

        var updater = new TasmotaDeviceUpdaterService(Logger, cf, Config);

        await updater.UpdateDevice(config);

        using var context = cf.CreateDbContext();
        var relays = context.Relays.ToArray();
        relays.Should().HaveCount(2);

        var mqtt = relays.Single(r => r.Name.EndsWith("MQTT", StringComparison.InvariantCulture));

        mqtt.Name.Should().Be("SmartPlug MQTT");
        mqtt.Type.Should().Be("Tasmota");
        var mqttCfg = ((JsonElement)mqtt.Config).Deserialize<TasmotaMqttClientConfig>();
        mqttCfg.Should().Be(new TasmotaMqttClientConfig
        {
            DeviceId = "tasmota_67890A",
            RelayId = 1,
        });

        var http = relays.Single(r => !r.Name.EndsWith("MQTT", StringComparison.InvariantCulture));

        http.Name.Should().Be("SmartPlug");
        http.Type.Should().Be("Tasmota");
        var httpCfg = ((JsonElement)http.Config).Deserialize<TasmotaHttpClientConfig>();
        httpCfg.Should().Be(new TasmotaHttpClientConfig
        {
            Host = "192.168.1.101",
            RelayId = 1,
        });
    }

    [Test]
    public async Task ChangedDeviceMessageShouldUpdateHttpDeviceIpTest()
    {
        var config = JsonSerializer.Deserialize<TasmotaDiscoveryMessage>(SmartPlug)!;
        var cf = CreateContextFactory();
        using (var context = cf.CreateDbContext())
        {
            context.Relays.Add(new RelayEntry
            {
                Id = Guid.NewGuid(),
                Type = "Tasmota",
                Name = "SmartPlug",
                Config = new TasmotaHttpClientConfig
                {
                    Host = "192.168.1.10",
                    RelayId = 1,
                }
            });
            await context.SaveChangesAsync();
        }

        var updater = new TasmotaDeviceUpdaterService(Logger, cf, Config);

        await updater.UpdateDevice(config);

        using var ctx = cf.CreateDbContext();
        var relays = ctx.Relays.ToArray();
        relays.Should().HaveCount(2);

        var http = relays.Single(r => !r.Name.EndsWith("MQTT", StringComparison.InvariantCulture));

        http.Name.Should().Be("SmartPlug");
        http.Type.Should().Be("Tasmota");
        var httpCfg = ((JsonElement)http.Config).Deserialize<TasmotaHttpClientConfig>();
        httpCfg.Should().Be(new TasmotaHttpClientConfig
        {
            Host = "192.168.1.101",
            RelayId = 1,
        });
    }

    [Test]
    public async Task ChangedDeviceMessageShouldUpdateMqttDeviceNameTest()
    {
        var config = JsonSerializer.Deserialize<TasmotaDiscoveryMessage>(SmartPlug)!;
        var cf = CreateContextFactory();
        using (var context = cf.CreateDbContext())
        {

            context.Relays.Add(new RelayEntry
            {
                Id = Guid.NewGuid(),
                Type = "Tasmota",
                Name = "DumbPlug",
                Config = new TasmotaMqttClientConfig
                {
                    DeviceId = "tasmota_67890A",
                    RelayId = 1,
                }
            });
            await context.SaveChangesAsync();
        }

        var updater = new TasmotaDeviceUpdaterService(Logger, cf, Config);

        await updater.UpdateDevice(config);

        using var ctx = cf.CreateDbContext();
        var relays = ctx.Relays.ToArray();
        relays.Should().HaveCount(2);

        var mqtt = relays.Single(r => r.Name.EndsWith("MQTT", StringComparison.InvariantCulture));

        mqtt.Name.Should().Be("SmartPlug MQTT");
        mqtt.Type.Should().Be("Tasmota");
        var mqttCfg = ((JsonElement)mqtt.Config).Deserialize<TasmotaMqttClientConfig>();
        mqttCfg.Should().Be(new TasmotaMqttClientConfig
        {
            DeviceId = "tasmota_67890A",
            RelayId = 1,
        });
    }

    [Test]
    public async Task UnknownDeviceWithTwoRelaysMessageShouldAddTwoNewDevicesTest()
    {
        var config = JsonSerializer.Deserialize<TasmotaDiscoveryMessage>(LightSwitch)!;
        var cf = CreateContextFactory();
        var updater = new TasmotaDeviceUpdaterService(Logger, cf, Config);

        await updater.UpdateDevice(config);

        using var context = cf.CreateDbContext();
        var relays = context.Relays.ToArray();
        relays.Should().HaveCount(4);

        using (new AssertionScope())
        {
            var mqtt1 = relays.Single(r => r.Name.EndsWith("-1 MQTT", StringComparison.InvariantCulture));

            mqtt1.Name.Should().Be("TasmotaLightSwitch1-1 MQTT");
            mqtt1.Type.Should().Be("Tasmota");
            var mqtt1Cfg = ((JsonElement)mqtt1.Config).Deserialize<TasmotaMqttClientConfig>();
            mqtt1Cfg.Should().Be(new TasmotaMqttClientConfig
            {
                DeviceId = "tasmota_334455",
                RelayId = 1,
            });

            var mqtt2 = relays.Single(r => r.Name.EndsWith("-2 MQTT", StringComparison.InvariantCulture));

            mqtt2.Name.Should().Be("TasmotaLightSwitch1-2 MQTT");
            mqtt2.Type.Should().Be("Tasmota");
            var mqtt2Cfg = ((JsonElement)mqtt2.Config).Deserialize<TasmotaMqttClientConfig>();
            mqtt2Cfg.Should().Be(new TasmotaMqttClientConfig
            {
                DeviceId = "tasmota_334455",
                RelayId = 2,
            });

            var http1 = relays.Single(r => r.Name.EndsWith("-1", StringComparison.InvariantCulture));

            http1.Name.Should().Be("TasmotaLightSwitch1-1");
            http1.Type.Should().Be("Tasmota");
            var http1Cfg = ((JsonElement)http1.Config).Deserialize<TasmotaHttpClientConfig>();
            http1Cfg.Should().Be(new TasmotaHttpClientConfig
            {
                Host = "192.168.1.102",
                RelayId = 1,
            });

            var http2 = relays.Single(r => r.Name.EndsWith("-2", StringComparison.InvariantCulture));

            http2.Name.Should().Be("TasmotaLightSwitch1-2");
            http2.Type.Should().Be("Tasmota");
            var http2Cfg = ((JsonElement)http2.Config).Deserialize<TasmotaHttpClientConfig>();
            http2Cfg.Should().Be(new TasmotaHttpClientConfig
            {
                Host = "192.168.1.102",
                RelayId = 2,
            });
        }
    }

    [Test]
    public async Task UpdateddRelayToDeviceWithMissingRelaysShouldAddRelaysTest()
    {
        var config = JsonSerializer.Deserialize<TasmotaDiscoveryMessage>(LightSwitch)!;
        var cf = CreateContextFactory();
        using (var context = cf.CreateDbContext())
        {
            context.Relays.Add(new RelayEntry
            {
                Id = Guid.NewGuid(),
                Type = "Tasmota",
                Name = "TasmotaLightSwitch1 MQTT",
                Config = new TasmotaMqttClientConfig
                {
                    DeviceId = "tasmota_334455",
                    RelayId = 1,
                },
            });

            context.Relays.Add(new RelayEntry
            {
                Id = Guid.NewGuid(),
                Type = "Tasmota",
                Name = "TasmotaLightSwitch1-1",
                Config = new TasmotaHttpClientConfig
                {
                    Host = "192.168.1.101",
                    RelayId = 1,
                },
            });

            await context.SaveChangesAsync();
        }

        var updater = new TasmotaDeviceUpdaterService(Logger, cf, Config);

        await updater.UpdateDevice(config);

        using var ctx = cf.CreateDbContext();
        var relays = ctx.Relays.ToArray();
        relays.Should().HaveCount(4);

        using (new AssertionScope())
        {
            var mqtt1 = relays.Single(r => r.Name.EndsWith("-1 MQTT", StringComparison.InvariantCulture));

            mqtt1.Name.Should().Be("TasmotaLightSwitch1-1 MQTT");
            mqtt1.Type.Should().Be("Tasmota");
            var mqtt1Cfg = ((JsonElement)mqtt1.Config).Deserialize<TasmotaMqttClientConfig>();
            mqtt1Cfg.Should().Be(new TasmotaMqttClientConfig
            {
                DeviceId = "tasmota_334455",
                RelayId = 1,
            });

            var mqtt2 = relays.Single(r => r.Name.EndsWith("-2 MQTT", StringComparison.InvariantCulture));

            mqtt2.Name.Should().Be("TasmotaLightSwitch1-2 MQTT");
            mqtt2.Type.Should().Be("Tasmota");
            var mqtt2Cfg = ((JsonElement)mqtt2.Config).Deserialize<TasmotaMqttClientConfig>();
            mqtt2Cfg.Should().Be(new TasmotaMqttClientConfig
            {
                DeviceId = "tasmota_334455",
                RelayId = 2,
            });

            var http1 = relays.Single(r => r.Name.EndsWith("-1", StringComparison.InvariantCulture));

            http1.Name.Should().Be("TasmotaLightSwitch1-1");
            http1.Type.Should().Be("Tasmota");
            var http1Cfg = ((JsonElement)http1.Config).Deserialize<TasmotaHttpClientConfig>();
            http1Cfg.Should().Be(new TasmotaHttpClientConfig
            {
                Host = "192.168.1.102",
                RelayId = 1,
            });

            var http2 = relays.Single(r => r.Name.EndsWith("-2", StringComparison.InvariantCulture));

            http2.Name.Should().Be("TasmotaLightSwitch1-2");
            http2.Type.Should().Be("Tasmota");
            var http2Cfg = ((JsonElement)http2.Config).Deserialize<TasmotaHttpClientConfig>();
            http2Cfg.Should().Be(new TasmotaHttpClientConfig
            {
                Host = "192.168.1.102",
                RelayId = 2,
            });
        }
    }
}
