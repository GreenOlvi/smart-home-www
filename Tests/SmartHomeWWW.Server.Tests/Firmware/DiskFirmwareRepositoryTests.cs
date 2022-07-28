using System;
using FluentAssertions;
using NUnit.Framework;
using SmartHomeWWW.Server.Firmwares;

namespace SmartHomeWWW.Server.Tests.Firmware;

[TestFixture]
public class DiskFirmwareRepositoryTests
{
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
