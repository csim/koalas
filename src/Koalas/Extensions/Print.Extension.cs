namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static partial class PrintExtension
{
    private const int _defaultDisplayWidth = 120;
    private const int _defaultItemLimit = 500;

    public static void Print(this string source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: false, maxTotalWidth);
    }

    public static void Print(this int source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: false, maxTotalWidth);
    }

    public static void Print(this float source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static void Print(this double source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static void Print(this decimal source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static void Print(this DateTime source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static void Print(this object source,
                             string label = null,
                             int? labelWidth = null,
                             bool literal = false,
                             int maxTotalWidth = _defaultDisplayWidth)
    {
        if (literal) source = source.ToCSharpLiteral();

        string formatted = Format(source, label, labelWidth).TrimEnd();

        Debug.WriteLine(formatted);
        Console.WriteLine(formatted);
        PrintEndSeparator();
    }

    public static void Print<T, TSelect>(this IEnumerable<T> items,
                                         Func<T, TSelect> select,
                                         Func<TSelect, bool> where = null,
                                         int limit = _defaultItemLimit)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        list.PrintHeadTail(i => Console.WriteLine(i), select, where, limit);
    }

    public static void Print<T>(this IEnumerable<T> items,
                                Func<T, bool> where = null,
                                int limit = _defaultItemLimit)
    {
        items.PrintHeadTail(i => Console.WriteLine(i), where, limit);
    }

    public static void PrintLiteral(this object source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static void PrintLiteral(this string source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth)
    {
        Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static void PrintLiteral<T, TSelect>(this IEnumerable<T> items,
                                                Func<T, TSelect> select,
                                                Func<TSelect, bool> where = null,
                                                int limit = _defaultItemLimit)
    {
        items.PrintHeadTail(i => Console.WriteLine(i.ToCSharpLiteral()), select, where, limit);
    }

    public static void PrintLiteral<T>(this IEnumerable<T> items,
                                       Func<T, bool> where = null,
                                       int limit = _defaultItemLimit)
    {
        items.PrintHeadTail(i => Console.WriteLine(i.ToCSharpLiteral()), where, limit);
    }

    public static void PrintMessage<T>(this IEnumerable<T> items, string message)
    {
        message.Print();
    }
}

public static partial class PrintExtension
{
    public static void PrintJson(this object subject, string label = null, int? labelWidth = null, Formatting format = Formatting.Indented)
    {
        FormatJson(subject, label, labelWidth, format: format).Print();
    }

    public static void PrintJson<T, TSelect>(this IEnumerable<T> items,
                                             Func<T, TSelect> select,
                                             Func<TSelect, bool> where = null,
                                             int limit = _defaultItemLimit)
    {
        items.Select(select)
             .PrintJson(limit: limit, where: where);
    }

    public static void PrintJson<T>(this IEnumerable<T> items,
                                    Func<T, bool> where = null,
                                    Formatting format = Formatting.Indented,
                                    int limit = _defaultItemLimit)
    {
        IReadOnlyList<T> list = where == null
                                    ? items.ToReadOnlyList()
                                    : items.Where(where).ToReadOnlyList();


        Console.WriteLine(FormatJson(list.Take(limit), format: format));

        if (list.Count > limit) Console.WriteLine($" ... {list.Count - limit:N0} more ...");

        PrintEndSeparator();
    }

    public static void PrintJsonLine<T, TSelect>(this IEnumerable<T> items,
                                                 Func<T, TSelect> select,
                                                 Func<TSelect, bool> where = null,
                                                 int limit = _defaultItemLimit)
    {
        items.Select(select)
             .PrintJsonLine(limit: limit, where: where);
    }

    public static void PrintJsonLine<T>(this IEnumerable<T> items,
                                        Func<T, bool> where = null,
                                        int limit = _defaultItemLimit)
    {
        items.PrintJson(where, Formatting.None, limit);
    }
}

public static partial class PrintExtension
{
    public static void PrintCount<T>(this IEnumerable<T> items, string label = null)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Count.ToString("N0").Print(label);
    }

    public static void PrintDistinctCount<T>(this IEnumerable<T> items, string label = null)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Distinct().Count().ToString("N0").Print(label);
    }
}

public static partial class PrintExtension
{
    private static void PrintHeadTail<T>(this IEnumerable<T> items,
                                         Action<T> output,
                                         Func<T, bool> where = null,
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

    private static void PrintHeadTail<T, TSelect>(this IEnumerable<T> items,
                                                  Action<TSelect> output,
                                                  Func<T, TSelect> select,
                                                  Func<TSelect, bool> where = null,
                                                  int limit = _defaultItemLimit)
    {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        list.Select(select)
            .PrintHeadTail(output, where, limit);
    }
}

public static partial class PrintExtension
{
    private static string CallerName()
    {
#if !DEBUG
        return null;
#endif

        try
        {
            StackTrace trace = new(0, true);
            string filename = null;
            int? lineNumber = null;

            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                if (frame == null) continue;


                MethodBase method = frame.GetMethod();
                if (method == null) continue;

                Type decaringType = method.DeclaringType;
                if (decaringType == null) continue;

                if (decaringType == typeof(PrintExtension)) continue;

                if (method.Name == "Start") continue;                     // Async state machine method
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

                if (filename == null) lineNumber = frame.GetFileLineNumber();

                if (lineNumber is null or 0) continue;

                return $"{method.DeclaringType?.Name}.{method.Name}():{lineNumber,-4}";
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static void PrintEndSeparator()
    {
        string callerName = CallerName();

        if (!string.IsNullOrEmpty(callerName))
        {
            Console.WriteLine($"--- {callerName,_defaultDisplayWidth - 3}");

            return;
        }

        Console.WriteLine("---");
    }
}

public static partial class PrintExtension
{
    private static string Format(object source,
                                 string label = null,
                                 int? labelWidth = null)
    {
        string output = source?.ToString();

        output ??= "<null>";
        output = output.TrimEnd();

        if (label == null) return output;

        if (labelWidth != null) label = label.PadRight(labelWidth.Value);

        return $"{label}: {output.IndentSkipFirstLine(label.Length + 2)}";
    }

    private static string FormatJson(object source,
                                     string label = null,
                                     int? labelWidth = null,
                                     Formatting format = Formatting.Indented)
    {
        string output = source is IToJson json
                            ? json.ToJson().ToString(format)
                            : JsonConvert.SerializeObject(source, format);

        return Format(output, label, labelWidth);
    }
}

public interface IToJson
{
    JObject ToJson();
}