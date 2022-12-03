namespace Koalas;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public static partial class KoalasExtensions
{
    private static int? _koalasDefaultMaxParallel;

    public static IEnumerable<T> Collect<T>(this IEnumerable<T> items)
    {
        return items.Where(i => i != null);
    }


    public static IReadOnlyList<T> ForAllParallel<T>(this IEnumerable<T> items,
                                                     Action<T> action,
                                                     int? maxParallel = null)
    {
        var list = items.CoerceList();

        list.AsParallel()
            .AsOrdered()
            .WithDegreeOfParallelism(maxParallel ?? KoalasDefaultMaxParallel).ForAll(item => action(item));

        return list;
    }

    public static IEnumerable<T> Head<T>(this IEnumerable<T> items, int size = 10)
    {
        return items.Take(size);
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
        return items.Select(item => JsonConvert.DeserializeObject<TTarget>(item));
    }

    public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> items, int size)
    {
        var list = items.CoerceList();

        for (int i = 0; i < Math.Ceiling(list.Count / (double)size); i++)
        {
            yield return new List<T>(list.Skip(size * i).Take(size));
        }
    }

    public static IEnumerable<T> Pause<T>(this IEnumerable<T> items, int milliseconds = 1000)
    {
        Console.WriteLine("Press enter to continue...");
        Console.ReadLine();

        return items;
    }



    public static IEnumerable<TTarget> SelectManyParallel<T, TTarget>(this IEnumerable<T> items,
                                                                      Func<T, IEnumerable<TTarget>> transform,
                                                                      int? maxParallel = null)
    {
        return items.AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(maxParallel ?? KoalasDefaultMaxParallel)
                    .SelectMany(i => transform(i));
    }

    public static IEnumerable<TTarget> SelectParallel<T, TTarget>(this IEnumerable<T> items,
                                                                  Func<T, TTarget> transform,
                                                                  int? maxParallel = null)
    {
        return items.AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(maxParallel ?? KoalasDefaultMaxParallel)
                    .Select(i => transform(i));
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
        var list = items.CoerceList();
        var listTotalCount = list.Count;
        var skip = Math.Max(0, listTotalCount - size);

        return list.Skip(skip);
    }



    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    private static int KoalasDefaultMaxParallel => _koalasDefaultMaxParallel ??= Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.5));
}


public class KoalasFileInfo
{
    public string Content { get; set; }

    public FileInfo File { get; set; }
}