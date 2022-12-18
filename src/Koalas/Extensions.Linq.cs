namespace Koalas;

using System;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionsLinq {
    private static int? _koalasDefaultMaxParallel;

    private static int KoalasDefaultMaxParallel => _koalasDefaultMaxParallel ??= Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.5));

    public static IReadOnlyList<T> CoerceToList<T>(this IEnumerable<T> items) {
        return items as IReadOnlyList<T> ?? items.ToList();
    }

    public static IEnumerable<T> Collect<T>(this IEnumerable<T> items) {
        return items.Where(i => i != null);
    }

    public static IReadOnlyList<T> ForAll<T>(this IEnumerable<T> items,
                                             Action<T> action) {
        IReadOnlyList<T> list = items.CoerceToList();
        foreach (T item in list) {
            action(item);
        }

        return list;
    }

    public static IReadOnlyList<T> ForAllParallel<T>(this IEnumerable<T> items,
                                                     Action<T> action,
                                                     int? maxParallel = null) {
        IReadOnlyList<T> list = items.CoerceToList();

        list.AsParallel()
            .AsOrdered()
            .WithDegreeOfParallelism(maxParallel ?? KoalasDefaultMaxParallel)
            .ForAll(action);

        return list;
    }

    public static IEnumerable<T> Head<T>(this IEnumerable<T> items, int size = 10) {
        return items.Take(size);
    }

    public static IEnumerable<T> Page<T>(this IEnumerable<T> items, int page, int pageSize) {
        return items.Skip(pageSize * page).Take(pageSize);
    }

    public static int PageCount<T>(this IEnumerable<T> items, int pageSize) {
        return Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDouble(pageSize)));
    }

    public static IEnumerable<TTarget> SelectManyParallel<T, TTarget>(this IEnumerable<T> items,
                                                                      Func<T, IEnumerable<TTarget>> transform,
                                                                      int? maxParallel = null) {
        return items.AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(maxParallel ?? KoalasDefaultMaxParallel)
                    .SelectMany(transform);
    }

    public static IEnumerable<TTarget> SelectParallel<T, TTarget>(this IEnumerable<T> items,
                                                                  Func<T, TTarget> transform,
                                                                  int? maxParallel = null) {
        return items.AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(maxParallel ?? KoalasDefaultMaxParallel)
                    .Select(transform);
    }

    public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> items, int skip, int take) {
        return items.Skip(skip).Take(take);
    }

    public static IEnumerable<T> Tail<T>(this IEnumerable<T> items, int size) {
        IReadOnlyList<T> list = items.CoerceToList();
        int listTotalCount = list.Count;
        int skip = Math.Max(0, listTotalCount - size);

        return list.Skip(skip);
    }

    public static IEnumerable<T> Yield<T>(this T item) {
        yield return item;
    }
}



