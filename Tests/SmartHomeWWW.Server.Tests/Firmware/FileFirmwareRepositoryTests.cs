using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Firmwares;

namespace SmartHomeWWW.Server.Tests.Firmware;

[TestFixture]
public class FileFirmwareRepositoryTests
{
    [TestCase("firmware.0.0.4.bin", "0.0.4")]
    [TestCase("firmware.1.0.0-alpha.bin", "1.0.0-alpha")]
    [TestCase("firmware.1.bin", null)]
    public void ExtractVersionFromFileName(string filename, string? expectedVersion)
    {
        var success = FileFirmwareRepository.TryExtractVersionFromFileName(filename, out var version);
        if (expectedVersion is not null)
        {
            success.Should().BeTrue();
            version.Should().Be(FirmwareVersion.Parse(expectedVersion));
        }
        else
        {
            success.Should().BeFalse();
        }
    }

    private readonly NullLogger<FileFirmwareRepository> _logger = NullLogger<FileFirmwareRepository>.Instance;

    [Test]
    public void GetAllFirmwaresTest()
    {
        var fs = new MockFileSystem();
        fs.AddFile("/firmwares/firmware.1.0.0.bin", new MockFileData("Binary data of firmware v1.0.0"));
        fs.AddFile("/firmwares/firmware.1.0.1-alpha.bin", new MockFileData("Binary data of firmware v1.0.1-alpha"));
        fs.AddFile("/firmwares/firmware.0.1.0-debug.bin", new MockFileData("Binary data of firmware v0.1.0-debug"));
        fs.AddFile("/firmwares/other_file.txt", new MockFileData("Lorem ipsum"));

        var config = new FirmwaresConfig { Path = @"/firmwares" };

        var repo = new FileFirmwareRepository(_logger, config.AsOptionsSnapshot(), fs);

        var firmwares = repo.GetAllFirmwares();
        firmwares.Should().NotBeNull();
        firmwares.Should().HaveCount(3);

        firmwares.Select(f => f.Version)
            .OrderBy(v => v.Prefix)
            .Should().BeEquivalentTo(new[]
            {
                FirmwareVersion.Parse("0.1.0-debug"),
                FirmwareVersion.Parse("1.0.0"),
                FirmwareVersion.Parse("1.0.1-alpha"),
            });

        var vDebug = firmwares.Single(f => f.Version.Prefix == new Version("0.1.0"));
        vDebug.Channel.Should().Be(UpdateChannel.Unknown);

        var v100 = firmwares.Single(f => f.Version.Prefix == new Version("1.0.0"));
        v100.Channel.Should().Be(UpdateChannel.Stable);

        var v101 = firmwares.Single(f => f.Version.Prefix == new Version("1.0.1"));
        v101.Channel.Should().Be(UpdateChannel.Alpha);
    }

    [Test]
    public void GetFirmwaresByChannelTest()
    {
        var fs = new MockFileSystem();
        fs.AddFile("/firmwares/firmware.1.0.0.bin", new MockFileData("Binary data of firmware v1.0.0"));
        fs.AddFile("/firmwares/firmware.1.0.1-alpha.bin", new MockFileData("Binary data of firmware v1.0.1-alpha"));
        fs.AddFile("/firmwares/other_file.txt", new MockFileData("Lorem ipsum"));

        var config = new FirmwaresConfig { Path = @"/firmwares" };

        IFirmwareRepository repo = new FileFirmwareRepository(_logger, config.AsOptionsSnapshot(), fs);

        var firmwares = repo.GetFirmwares(UpdateChannel.Alpha);
        firmwares.Should().NotBeNull();
        firmwares.Should().HaveCount(1);

        firmwares.Select(f => f.Version)
            .OrderBy(v => v.Prefix)
            .Should().BeEquivalentTo(new[]
            {
                FirmwareVersion.Parse("1.0.1-alpha"),
            });
    }

    [Test]
    public void GetCurrentFirmwareByChannelTest()
    {
        var fs = new MockFileSystem();
        fs.AddFile("/firmwares/firmware.1.0.0.bin", new MockFileData("Binary data of firmware v1.0.0"));
        fs.AddFile("/firmwares/firmware.0.3.0-alpha.bin", new MockFileData("Binary data of firmware v0.3.0-alpha"));
        fs.AddFile("/firmwares/firmware.1.0.0-alpha.bin", new MockFileData("Binary data of firmware v1.0.0-alpha"));
        fs.AddFile("/firmwares/firmware.1.0.1-alpha.bin", new MockFileData("Binary data of firmware v1.0.1-alpha"));
        fs.AddFile("/firmwares/other_file.txt", new MockFileData("Lorem ipsum"));

        var config = new FirmwaresConfig { Path = @"/firmwares" };

        IFirmwareRepository repo = new FileFirmwareRepository(_logger, config.AsOptionsSnapshot(), fs);

        var firmware = repo.GetCurrentFirmware(UpdateChannel.Alpha);
        firmware.Should().NotBeNull();
        firmware!.Version.Should().Be(FirmwareVersion.Parse("1.0.1-alpha"));
        firmware.Channel.Should().Be(UpdateChannel.Alpha);
    }
}
