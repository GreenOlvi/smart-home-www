using Dunet;

namespace SmartHomeWWW.Core.Utils.Functional;

[Union]
public partial record Option<T>
{
    public partial record Some(T Value);
    public partial record None();
}

public static class OptionHelper
{
    public static Option<T> ToOption<T>(this T? value) => value is null ? new Option<T>.None() : new Option<T>.Some(value);
}
