using FluentAssertions;
using NUnit.Framework;
using SmartHomeWWW.Core.Firmwares;

namespace SmartHomeWWW.Core.Tests.Firmwares;

[TestFixture]
public class FirmwareVersionTests
{
    [TestCase("0.0.4", "0.0.4", null)]
    [TestCase("1.0.0-alpha", "1.0.0", "alpha")]
    public void ParseTests(string input, string expectedPrefix, string? expectedSuffix)
    {
        var result = FirmwareVersion.Parse(input);
        result.Prefix.Should().Be(new Version(expectedPrefix));
        result.Suffix.Should().Be(expectedSuffix);
    }

    [TestCase("0.0.4", "0.0.4", null)]
    [TestCase("1.0.0-alpha", "1.0.0", "alpha")]
    [TestCase("1.a-nonsense", null, null)]
    public void TryParseTests(string input, string? expectedPrefix, string? expectedSuffix)
    {
        var success = FirmwareVersion.TryParse(input, out var result);

        if (expectedPrefix is not null)
        {
            success.Should().BeTrue();
            var version = new Version(expectedPrefix);
            result.Prefix.Should().Be(version);
            result.Suffix.Should().Be(expectedSuffix);

            var expected = new FirmwareVersion { Prefix = version, Suffix = expectedSuffix };
            (result == expected).Should().BeTrue();
        }
        else
        {
            success.Should().BeFalse();
        }
    }
}
