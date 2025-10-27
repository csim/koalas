namespace Koalas.Extensions;

public static partial class LinqExtensions
{
    public static IEnumerable<string> AddDelimiter(
        this IEnumerable<string>? subject,
        string separator
    )
    {
        if (subject == null)
        {
            yield break;
        }

        bool first = true;
        foreach (string item in subject)
        {
            if (!first)
            {
                yield return separator;
            }

            yield return item;

            first = false;
        }
    }

    public static void AddDistinctItem<T>(this IList<T> list, T value)
    {
        if (!list.Contains(value))
        {
            list.Add(value);
        }
    }

    public static IList<T> AddItem<T>(this IList<T> items, T item)
    {
        items.Add(item);

        return items;
    }

    public static IEnumerable<string> AddPrefix(this IEnumerable<string>? subject, string prefix)
    {
        return subject == null ? [] : subject.Select(line => $"{prefix}{line}");
    }

    public static IEnumerable<string> AddPrefixDelimiter(
        this IEnumerable<string>? subject,
        string prefixDelimiter
    )
    {
        return subject == null
            ? []
            : subject.Select((line, index) => index == 0 ? line : $"{prefixDelimiter}{line}");
    }

    public static IEnumerable<string> AddSuffix(this IEnumerable<string>? subject, string suffix)
    {
        return subject?.Select(line => $"{line}{suffix}") ?? [];
    }

    public static IEnumerable<string> AddSuffixDelimiter(
        this IEnumerable<string>? subject,
        string suffixDelimiter
    )
    {
        subject = subject.ToReadOnlyList();
        int lastIndex = subject.Count() - 1;

        return subject.Select(
            (line, index) => index == lastIndex ? line : $"{line}{suffixDelimiter}"
        );
    }

    public static bool Any<T>(this IEnumerable<T>? items, T? item)
    {
        return items?.Any(i => (i == null && item == null) || i?.Equals(item) == true) ?? false;
    }

    public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (onNext == null)
        {
            throw new ArgumentNullException(nameof(onNext));
        }

        foreach (TSource? item in source)
        {
            onNext(item);
        }
    }

    public static IEnumerable<T> Head<T>(this IEnumerable<T> items, int size = 10)
    {
        return items.Take(size);
    }

    public static bool None<T>(this IEnumerable<T> items)
    {
        return !items.Any();
    }

    public static bool None<T>(this IEnumerable<T> items, T? item)
    {
        return !items.Any(item);
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

    public static IEnumerable<T> ParseJson<T>(this IEnumerable<string>? items)
    {
        return items?.Select(static i =>
                JsonSerializer.Deserialize<T>(i)
                ?? throw new ArgumentOutOfRangeException(typeof(T).Name)
            ) ?? [];
    }

    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> items, int size)
    {
        int partitionIndex = 0;
        while (true)
        {
            IEnumerable<T> ret = items.Skip(size * partitionIndex).Take(size);
            if (ret.None())
            {
                yield break;
            }

            yield return ret;

            partitionIndex++;
        }
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

    public static T[] ToArrayOrEmpty<T>(this IEnumerable<T>? subject)
    {
        return subject as T[] ?? subject?.ToArray() ?? [];
    }

    public static IReadOnlyList<T> ToDistinctReadOnlyList<T>(this IEnumerable<T>? subject)
    {
        return (subject?.Distinct()).ToReadOnlyList();
    }

    public static string ToJoinNewlineString(
        this IEnumerable<string>? subject,
        string? prefix = null,
        string? suffix = null
    )
    {
        if (subject == null)
        {
            return string.Empty;
        }

        IEnumerable<string> lines =
            prefix == null && suffix == null
                ? subject
                : subject.Select(line => $"{prefix}{line}{suffix}");

        return string.Join(Environment.NewLine, lines);
    }

    public static string ToJoinString(this IEnumerable<char> subject)
    {
        return string.Concat(subject);
    }

    public static string ToJoinString(this IEnumerable<string> subject)
    {
        return string.Concat(subject);
    }

    public static string ToJoinString(this IEnumerable<string?>? subject, string delimiter)
    {
        return subject == null ? string.Empty : string.Join(delimiter, subject);
    }

    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T>? subject)
    {
        return subject as IReadOnlyList<T> ?? (IReadOnlyList<T>?)subject?.ToList() ?? [];
    }

    public static bool TryFirst<T>(this IEnumerable<T> subject, out T? item)
    {
        foreach (T i in subject)
        {
            item = i;

            return true;
        }

        item = default;

        return false;
    }

    public static bool TryFirst<T>(
        this IEnumerable<T> subject,
        Func<T, bool> predicate,
        out T? item
    )
    {
        foreach (T i in subject)
        {
            if (!predicate(i))
            {
                continue;
            }

            item = i;

            return true;
        }

        item = default;

        return false;
    }
}