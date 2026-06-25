namespace XcaXds.Shared.Extensions;

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
        var list = source.ToList();
        var rng = Random.Shared;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    /// <summary>
    /// Returns null if the input is null or empty, otherwise returns an array of the input.
    /// <para/>
    /// Useful for when you want null instead of an empty array, 
    /// or for cleanly null coalescing when doing LINQ-operations
    /// </summary>
    public static T[]? ToArrayOrNull<T>(this IEnumerable<T>? input)
    {
        return input == null || !input.Any() ? null : input.ToArray();
    }
}
