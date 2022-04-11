using System;
using NUnit.Framework;
using FluentAssertions;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Server.Relays;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using SmartHomeWWW.Server.Messages;

namespace SmartHomeWWW.Server.Tests.Relays
{
    [TestFixture]
    public class RelayFactoryTests
    {
        [Test]
        public void BuildRelayFromEntryHttpTest()
        {
            var entry = new RelayEntry
            {
                Id = Guid.Parse("3be25090-efe0-4b4e-a0d1-4218733ceecc"),
                Name = "Test relay",
                Type = "Tasmota",
                ConfigSerialized = @"{""Host"":""relay1.local"",""RelayId"":1}",
            };

            var tcf = new TasmotaClientFactory(
                NullLogger<TasmotaClientFactory>.Instance,
                new Mock<ILoggerFactory>().Object,
                new Mock<IHttpClientFactory>().Object,
                new Mock<IMessageBus>().Object);

            var factory = new RelayFactory(tcf);

            var relay = factory.Create(entry);

            relay.Should().BeOfType<TasmotaRelay>();
        }

        [Test]
        public void BuildRelayFromEntryMqttTest()
        {
            var entry = new RelayEntry
            {
                Id = Guid.Parse("3be25090-efe0-4b4e-a0d1-4218733ceecd"),
                Name = "Test relay",
                Type = "Tasmota",
                ConfigSerialized = @"{""Kind"":""Mqtt"",""DeviceId"":""tasmota_0A1B2C"",""RelayId"":1}",
            };

            var tcf = new TasmotaClientFactory(
                NullLogger<TasmotaClientFactory>.Instance,
                new Mock<ILoggerFactory>().Object,
                new Mock<IHttpClientFactory>().Object,
                new Mock<IMessageBus>().Object);

            var factory = new RelayFactory(tcf);

            var relay = factory.Create(entry);

            relay.Should().BeOfType<TasmotaRelay>();
        }
    }
}
