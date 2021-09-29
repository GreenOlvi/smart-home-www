using NUnit.Framework;
using FluentAssertions;
using System;
using SmartHomeCore.Firmwares;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartHomeWWWTests.Logic
{
    [TestFixture]
    public class DiskFirmwareRepositoryTests
    {
        [SetUp]
        public void Setup()
        {
            _repository = new DiskFirmwareRepository(NullLogger<DiskFirmwareRepository>.Instance, string.Empty);
        }

        private DiskFirmwareRepository _repository;

        [TestCase("firmware.0.0.4.bin", "0.0.4")]
        public void ExtractVersionFromFileName(string filename, string expectedVersion)
        {
            var success = DiskFirmwareRepository.TryExtractVersionFromFileName(filename, out var version);
            if (expectedVersion is not null)
            {
                success.Should().BeTrue();
                version.Should().Be(new Version(expectedVersion));
            }
            else
            {
                success.Should().BeFalse();
            }
        }
    }
}
