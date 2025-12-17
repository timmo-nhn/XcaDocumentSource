namespace XcaXds.Commons.Extensions;

/// <summary>
/// <a href="https://stackoverflow.com/questions/2019417/how-to-access-random-item-in-list" />
/// </summary>
public static class EnumerableExtensions
{
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).Single();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }

    /// <summary>
    /// Filter an Enumerable based on if the condition evaluates to true<br>Essentially an if-statement with a .Where-statement inside
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Incoming Enumerable</param>
    /// <param name="condition">The expression that needs to evaluate to true for the filter predicate to apply</param>
    /// <param name="filterToApply">the predicate to filter each element in source</param>
    /// <returns></returns>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source,bool condition,Func<T, bool> filterToApply)
    {
        return condition ? source.Where(filterToApply) : source;
    }
}
