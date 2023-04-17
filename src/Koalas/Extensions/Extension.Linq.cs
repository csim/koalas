namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static partial class Extension {
    public static void AddDistinct<T>(this IList<T> list, T value) {
        if (!list.Contains(value)) {
            list.Add(value);
        }
    }

    public static void ForEach<TSource>(this IEnumerable<TSource> subject, Action<TSource> onNext) {
        if (subject == null) throw new ArgumentNullException(nameof(subject));
        if (onNext == null) throw new ArgumentNullException(nameof(onNext));

        foreach (TSource item in subject) {
            onNext(item);
        }
    }

    public static IEnumerable<T> Head<T>(this IEnumerable<T> items, int size = 10) {
        return items.Take(size);
    }

    public static bool IsNullOrEmpty(this string subject) {
        return string.IsNullOrEmpty(subject);
    }

    public static bool None<T>(this IEnumerable<T> items) {
        return !items.Any();
    }

    public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
        return !items.Any(predicate);
    }

    public static IEnumerable<string> NotNullOrEmpty(this IEnumerable<string> subject) {
        return subject.Where(s => !s.IsNullOrEmpty());
    }

    public static IEnumerable<T> Page<T>(this IEnumerable<T> items, int page, int pageSize) {
        return items.Skip(pageSize * page).Take(pageSize);
    }

    public static int PageCount<T>(this IEnumerable<T> items, int pageSize) {
        return Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDouble(pageSize)));
    }

    public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> items, int skip, int take) {
        return items.Skip(skip).Take(take);
    }

    public static IEnumerable<T> Tail<T>(this IEnumerable<T> items, int size) {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        int listTotalCount = list.Count;
        int skip = Math.Max(0, listTotalCount - size);

        return list.Skip(skip);
    }

    public static T[] ToArrayOrEmpty<T>(this IEnumerable<T> subject) {
        return subject as T[] ?? subject?.ToArray() ?? new T[0];
    }

    public static IReadOnlyList<T> ToDistinctReadOnlyList<T>(this IEnumerable<T> subject) {
        return (subject?.Distinct()).ToReadOnlyList();
    }

    public static string ToJoinString(this IEnumerable<string> subject, string delimiter) {
        return subject == null
                   ? string.Empty
                   : string.Join(delimiter, subject);
    }

    public static JArray ToJson(this IEnumerable<object> subjects) {
        return subjects == null
                   ? null
                   : JArray.FromObject(subjects.Select(s => s.ToJson()).ToArray());
    }

    public static JObject ToJson(this object subject) {
        return JObject.FromObject(subject);
    }

    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> subject) {
        return subject as IReadOnlyList<T> ?? (IReadOnlyList<T>)subject?.ToList() ?? new T[0];
    }

    public static IEnumerable<T> Yield<T>(this T subject) {
        yield return subject;
    }
}

public static partial class Extension {
    public static IEnumerable<TTarget> ParseJson<TTarget>(this IEnumerable<string> items) {
        return items.Select(JsonConvert.DeserializeObject<TTarget>);
    }

    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> items, int size) {
        items = items as IReadOnlyCollection<T> ?? items.ToList();

        var i = 0;
        while (true) {
            IEnumerable<T> ret = items.Skip(size * i).Take(size).ToList();
            if (ret.FirstOrDefault() == null) {
                break;
            }

            yield return ret;

            i++;
        }
    }

    public static IEnumerable<string> SerializeJson<T>(this IEnumerable<T> items, Formatting format = Formatting.None) {
        return items.Select(item => JsonConvert.SerializeObject(item, format));
    }
}