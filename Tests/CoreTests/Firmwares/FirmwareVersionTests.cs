using FluentAssertions;
using NUnit.Framework;
using SmartHomeWWW.Core.Firmwares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
