namespace Koalas;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public static partial class ExtensionsFiles {
    public static IEnumerable<TTarget> ParseJson<TTarget>(this IEnumerable<string> items) {
        return items.Select(JsonConvert.DeserializeObject<TTarget>);
    }

    public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> items, int size) {
        IReadOnlyList<T> list = items.CoerceToList();

        for (var i = 0; i < Math.Ceiling(list.Count / (double)size); i++) {
            yield return new List<T>(list.Skip(size * i).Take(size));
        }
    }

    public static IEnumerable<T> Pause<T>(this IEnumerable<T> items, int milliseconds = 1000) {
        Console.WriteLine("Press enter to continue...");
        Console.ReadLine();

        return items;
    }

    public static IEnumerable<string> SerializeJson<T>(this IEnumerable<T> items, Formatting format = Formatting.None) {
        return items.Select(item => JsonConvert.SerializeObject(item, format));
    }
}


