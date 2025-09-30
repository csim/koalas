namespace Koalas.Extensions;

public static partial class LinqExtension
{
    public static void AddDistinctItem<T>(this IList<T> list, T value)
    {
        if (!list.Contains(value)) list.Add(value);
    }

    public static IList<T> AddItem<T>(this IList<T> items, T item)
    {
        items.Add(item);

        return items;
    }

    public static bool Any<T>(this IEnumerable<T> items, T item)
    {
        return items.Any(i => (i == null && item == null) || i?.Equals(item) == true);
    }

    public static void ForEach<TSource>(this IEnumerable<TSource> subject, Action<TSource> onNext)
    {
        if (subject == null) throw new ArgumentNullException(nameof(subject));
        if (onNext == null) throw new ArgumentNullException(nameof(onNext));

        foreach (TSource item in subject)
        {
            onNext(item);
        }
    }

    public static IEnumerable<T> Head<T>(this IEnumerable<T> items, int size = 10)
    {
        return items.Take(size);
    }

    public static bool None<T>(this IEnumerable<T> items, T item)
    {
        return !items.Any(item);
    }

    public static bool None<T>(this IEnumerable<T> items)
    {
        return !items.Any();
    }

    public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        return !items.Any(predicate);
    }

    public static IEnumerable<string> NotNullOrEmpty(this IEnumerable<string> subject)
    {
        return subject.Where(s => !string.IsNullOrEmpty(s));
    }

    public static IEnumerable<T> Page<T>(this IEnumerable<T> items, int page, int pageSize)
    {
        return items.Skip(pageSize * page).Take(pageSize);
    }

    public static int PageCount<T>(this IEnumerable<T> items, int pageSize)
    {
        return Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDouble(pageSize)));
    }

    public static IEnumerable<TTarget> ParseJson<TTarget>(this IEnumerable<string> items)
    {
        return items.Select(JsonConvert.DeserializeObject<TTarget>);
    }

    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> items, int size)
    {
        items = items as IReadOnlyCollection<T> ?? items.ToList();

        int partitionIndex = 0;
        while (true)
        {
            IReadOnlyList<T> ret = items.Skip(size * partitionIndex)
                                        .Take(size)
                                        .ToList();
            if (ret.None()) yield break;

            yield return ret;

            partitionIndex++;
        }
    }

    public static IEnumerable<string> SerializeJson<T>(this IEnumerable<T> items, Formatting format = Formatting.None)
    {
        return items.Select(item => JsonConvert.SerializeObject(item, format));
    }

    public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> items, int skip, int take)
    {
        return items.Skip(skip).Take(take);
    }

    public static IEnumerable<T> Tail<T>(this IEnumerable<T> items, int size)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        int listTotalCount = list.Count;
        int skip = Math.Max(0, listTotalCount - size);

        return list.Skip(skip);
    }

    public static T[] ToArrayOrEmpty<T>(this IEnumerable<T> subject)
    {
        return subject as T[] ?? subject?.ToArray() ?? [];
    }

    public static IReadOnlyList<T> ToDistinctReadOnlyList<T>(this IEnumerable<T> subject)
    {
        return (subject?.Distinct()).ToReadOnlyList();
    }

    public static string ToJoinNewlineString(this IEnumerable<string> subject)
    {
        return string.Join(Environment.NewLine, subject);
    }

    public static string ToJoinString(this IEnumerable<char> subject)
    {
        return string.Concat(subject);
    }

    public static string ToJoinString(this IEnumerable<string> subject)
    {
        return string.Concat(subject);
    }

    public static string ToJoinString(this IEnumerable<string> subject, string delimiter)
    {
        return subject == null ? string.Empty : string.Join(delimiter, subject);
    }

    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> subject)
    {
        return subject as IReadOnlyList<T> ?? (IReadOnlyList<T>)subject?.ToList() ?? [];
    }

    public static bool TryFirst<T>(this IEnumerable<T> subject, out T item)
    {
        foreach (T i in subject)
        {
            item = i;

            return true;
        }

        item = default;

        return false;
    }

    public static bool TryFirst<T>(this IEnumerable<T> subject, Func<T, bool> predicate, out T item)
    {
        foreach (T i in subject)
        {
            if (!predicate(i)) continue;

            item = i;

            return true;
        }

        item = default;

        return false;
    }

    public static IEnumerable<T> Yield<T>(this T subject)
    {
        yield return subject;
    }
}
