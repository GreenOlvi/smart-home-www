namespace SmartHomeWWW.Core.Utils;

public static class FunctionalExtensions
{
    public static IEnumerable<T> Unpack<T>(this IEnumerable<(bool, T)> collection) =>
        collection.Where(p => p.Item1).Select(p => p.Item2);
}
