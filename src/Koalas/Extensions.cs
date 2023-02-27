namespace Koalas;

using System;
using System.Collections.Generic;

public static partial class ExtensionsFiles {
    public static IEnumerable<T> Pause<T>(this IEnumerable<T> items, int milliseconds = 1000) {
        Console.WriteLine("Press enter to continue...");
        Console.ReadLine();

        return items;
    }
}