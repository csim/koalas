namespace Koalas.Extensions;

#nullable enable


public static partial class PrintExtension
{
    // ReSharper disable once InconsistentNaming
    private const int _defaultItemLimit = 500;

    public static void Print(this object? source,
                             string? label = null)
    {
        PrintInternal(source.Render(), label, includeEndSeparator: true);
    }

    public static void PrintJson(this object? subject, string? label = null, Formatting format = Formatting.Indented)
    {
        PrintInternal(FormatJson(subject, label, format: format));
    }

    public static void PrintJsonLine(this object? subject, string? label = null)
    {
        subject.PrintJson(label: label, format: Formatting.None);
    }

    public static void PrintRaw(this object? source,
                                string? label = null)
    {
        PrintInternal(source.Render(), label, includeEndSeparator: false);
    }

    private static void PrintInternal(this string? source,
                                      string? label = null,
                                      bool includeEndSeparator = true)
    {
        string formatted = Format(source ?? "<null>", label).TrimEnd();

        Debug.WriteLine(formatted);
        Console.WriteLine(formatted);

        if (!includeEndSeparator) return;

        PrintEndSeparator();
    }
}

public static partial class PrintExtension
{
    public static void PrintCount<T>(this IEnumerable<T>? items, string? label = null)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Count.ToString("N0").Print(label);
    }

    public static void PrintDistinctCount<T>(this IEnumerable<T>? items, string? label = null)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Distinct().Count().ToString("N0").Print(label);
    }
}

public static partial class PrintExtension
{
    private static void PrintHeadTail<T>(this IEnumerable<T>? items,
                                         Action<T> output,
                                         Func<T, bool>? where = null,
                                         int limit = _defaultItemLimit)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        int headCount = list.Count;
        int tailCount = 0;

        if (list.Count > limit)
        {
            double size = limit / 2d;
            headCount = Convert.ToInt32(Math.Ceiling(size));
            tailCount = Convert.ToInt32(Math.Floor(size));
        }

        List<T> sources = where == null
                              ? list.ToList()
                              : list.Where(where).ToList();

        sources.Head(headCount).ForEach(output);

        if (tailCount > 0)
        {
            if (sources.Count > limit) Console.WriteLine($" ... {sources.Count - limit:N0} skipped ...");

            sources.Tail(tailCount).ForEach(output);
        }

        PrintEndSeparator();
    }

    private static void PrintHeadTail<T, TSelect>(this IEnumerable<T>? items,
                                                  Action<TSelect> output,
                                                  Func<T, TSelect> select,
                                                  Func<TSelect, bool>? where = null,
                                                  int limit = _defaultItemLimit)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        list.Select(select)
            .PrintHeadTail(output, where, limit);
    }
}

public static partial class PrintExtension
{
    private static string? CallerName()
    {
#if DEBUG
        try
        {
            StackTrace trace = new(0, true);
            string? filename = null;
            int? lineNumber = null;

            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame? frame = trace.GetFrame(i);
                if (frame == null) continue;

                MethodBase? method = frame.GetMethod();
                if (method == null) continue;

                Type? decaringType = method.DeclaringType;
                if (decaringType == null) continue;

                if (decaringType == typeof(PrintExtension)) continue;

                if (method.Name == "Start") continue; // Async state machine method

                if (decaringType.Name.Contains("CSharpKernel")) continue; // Notebook  method

                if (decaringType.Name.Contains("DisplayClass")) continue; // Notebook  method

                // Async state machine internal method
                if (method.Name == "MoveNext")
                {
                    // capture filename and lineNumber
                    filename = frame.GetFileName();
                    lineNumber = frame.GetFileLineNumber();

                    continue;
                }

                if (filename == null)
                {
                    filename = frame.GetFileName();
                    lineNumber = frame.GetFileLineNumber();
                }

                if (filename == null || lineNumber is null or 0) return null;

                return $"{Path.GetFileName(filename)}:{lineNumber,-4}";
            }
        }
        catch
        {
            // ignore
        }
#endif
        return null;
    }

    private static void PrintEndSeparator()
    {
        string? callerName = CallerName();

        if (!string.IsNullOrEmpty(callerName))
        {
            Console.WriteLine($"--- {callerName,96}");

            return;
        }

        Console.WriteLine("---");
    }
}

public static partial class PrintExtension
{
    private static string Format(string? source,
                                 string? label = null)
    {
        string output = source ?? "<null>";

        return label == null
                   ? output
                   : $"""
                      {label}: 
                      {output.Indent()}
                      """;
    }

    private static string FormatJson(object? source,
                                     string? label = null,
                                     Formatting format = Formatting.Indented)
    {
        string output = source is IToJson json
                            ? json.ToJson().ToString(format)
                            : JsonConvert.SerializeObject(source, format);

        return Format(output, label);
    }
}