namespace Koalas.Extensions;

public static class PrintExtensions
{
    // ReSharper disable once InconsistentNaming
    private const int _defaultItemLimit = 200;

    /// <summary>
    ///     Prints the source object to the console and debug output.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="label"></param>
    /// <param name="includeEndSeparator"></param>
    public static void Print(
        this object? source,
        string? label = null,
        bool includeEndSeparator = true
    )
    {
        PrintInternal(source.Render(), label, includeEndSeparator: includeEndSeparator);
    }

    /// <summary>
    ///     Prints the source string to the console and debug output.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="label"></param>
    /// <param name="includeEndSeparator"></param>
    public static void Print(
        this string? source,
        string? label = null,
        bool includeEndSeparator = true
    )
    {
        PrintInternal(source ?? string.Empty, label, includeEndSeparator: includeEndSeparator);
    }

    /// <summary>
    ///     Prints a collection of items to the console and debug output.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="label"></param>
    /// <param name="limit"></param>
    public static void Print<T>(
        this IEnumerable<T?>? items,
        string? label = null,
        int limit = _defaultItemLimit
    )
    {
        IReadOnlyList<T?> list = items.ToReadOnlyList();

        int headCount = list.Count;
        int tailCount = 0;

        if (list.Count > limit)
        {
            double size = limit / 2d;
            headCount = Convert.ToInt32(Math.Ceiling(size));
            tailCount = Convert.ToInt32(Math.Floor(size));
        }

        int position = 0;
        TextListBuilder? builder = TextListBuilder.Create();

        foreach (T? item in list.Head(headCount))
        {
            position++;
            builder.AddItem(item.Render(), id: position.ToString());
        }

        if (tailCount > 0)
        {
            if (list.Count > limit)
            {
                builder.AddItem(
                    $" ... {list.Count - limit:N0} skipped ...",
                    id: string.Empty,
                    separator: string.Empty
                );
            }

            position = list.Count - tailCount;
            foreach (T? item in list.Tail(tailCount))
            {
                position++;
                builder.AddItem(item.Render(), id: position.ToString());
            }
        }

        string formatted = Format(builder.Render(), label);
        Debug.WriteLine(formatted);
        Console.WriteLine(formatted);

        PrintEndSeparator();
    }

    /// <summary>
    ///     Prints the count of items in the collection to the console and debug output.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="label"></param>
    public static void PrintCount<T>(this IEnumerable<T>? items, string? label = null)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Count.Print(label);
    }

    /// <summary>
    ///     Prints the distinct count of items in the collection to the console and debug output.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="label"></param>
    public static void PrintDistinctCount<T>(this IEnumerable<T>? items, string? label = null)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Distinct().Count().Print(label);
    }

    /// <summary>
    ///     Prints the source object as JSON to the console and debug output.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="label"></param>
    /// <param name="format"></param>
    /// <param name="includeEndSeparator"></param>
    public static void PrintJson(
        this object? subject,
        string? label = null,
        Formatting format = Formatting.Indented,
        bool includeEndSeparator = true
    )
    {
        PrintInternal(
            FormatJson(subject, label, format: format),
            includeEndSeparator: includeEndSeparator
        );
    }

    /// <summary>
    ///     Prints the source object as a single line of JSON to the console and debug output.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="label"></param>
    /// <param name="includeEndSeparator"></param>
    public static void PrintJsonLine(
        this object? subject,
        string? label = null,
        bool includeEndSeparator = true
    )
    {
        subject.PrintJson(
            label: label,
            format: Formatting.None,
            includeEndSeparator: includeEndSeparator
        );
    }

    /// <summary>
    ///     Prints the source object as a raw string to the console and debug output without the end separator.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="label"></param>
    public static void PrintRaw(this object? source, string? label = null)
    {
        PrintInternal(source.Render(), label, includeEndSeparator: false);
    }

    /// <summary>
    ///     Returns the location of the caller in the format "FileName:LineNumber".
    /// </summary>
    /// <returns></returns>
    private static string? CallerLocation()
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
                if (frame == null)
                    continue;

                MethodBase? method = frame.GetMethod();
                if (method == null)
                    continue;

                Type? decaringType = method.DeclaringType;
                if (decaringType == null)
                    continue;

                if (decaringType == typeof(PrintExtensions))
                    continue;

                if (method.Name == "Start")
                    continue; // Async state machine method

                if (decaringType.Name.Contains("CSharpKernel"))
                    continue; // Notebook  method

                if (decaringType.Name.Contains("DisplayClass"))
                    continue; // Notebook  method

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

                return filename == null || lineNumber is null or 0
                    ? null
                    : $"{Path.GetFileName(filename)}:{lineNumber, -4}";
            }
        }
        catch
        {
            // ignore
        }
#endif
        return null;
    }

    private static string Format(string? source, string? label = null)
    {
        string output = source ?? string.Empty;

        output =
            label == null
                ? output
                : $"""
                    {label}:
                    {output.Indent(2)}
                    """;

        return output.TrimEnd();
    }

    private static string FormatJson(
        object? source,
        string? label = null,
        Formatting format = Formatting.Indented
    )
    {
        string? output = source is IToJson json
            ? json.ToJson().ToString(format)
            : source?.ToJsonString(format);

        return Format(output, label);
    }

    private static void PrintEndSeparator()
    {
        string? callerLocation = CallerLocation();

        if (string.IsNullOrEmpty(callerLocation))
        {
            Debug.WriteLine("---");
            Console.WriteLine("---");

            return;
        }

        string output = $"--- {callerLocation, 96}";

        Debug.WriteLine(output);
        Console.WriteLine(output);
    }

    private static void PrintInternal(
        this string source,
        string? label = null,
        bool includeEndSeparator = true
    )
    {
        string formatted = Format(source, label);

        Debug.WriteLine(formatted);
        Console.WriteLine(formatted);

        if (includeEndSeparator)
            PrintEndSeparator();
    }
}
