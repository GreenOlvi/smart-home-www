using Microsoft.Extensions.Options;

namespace SmartHomeWWW.Server.Tests;
public static class TestHelpers
{
    public static IOptionsSnapshot<TOptions> AsOptionsSnapshot<TOptions>(this TOptions options) where TOptions : class
    {
        var opts = Substitute.For<IOptionsSnapshot<TOptions>>();
        opts.Value.Returns(options);
        return opts;
    }

    public static IOptions<TOptions> AsOptions<TOptions>(this TOptions options) where TOptions : class
    {
        var opts = Substitute.For<IOptions<TOptions>>();
        opts.Value.Returns(options);
        return opts;
    }
}
