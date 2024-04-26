namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static partial class PrintExtension {
    private const int _defaultDisplayWidth = 120;
    private const int _defaultItemLimit = 500;

    public static object Print(this string source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth) {
        return Print(source, label, labelWidth, literal: false, maxTotalWidth);
    }

    public static object Print(this int source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth) {
        return Print(source, label, labelWidth, literal: false, maxTotalWidth);
    }

    public static object Print(this float source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth) {
        return Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static object Print(this double source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth) {
        return Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static object Print(this decimal source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth) {
        return Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static object Print(this DateTime source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth) {
        return Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static object Print(this object source,
                               string label = null,
                               int? labelWidth = null,
                               bool literal = false,
                               int maxTotalWidth = _defaultDisplayWidth) {
        if (literal) {
            source = source.ToCSharpLiteral();
        }

        string formatted = Format(source, label, labelWidth, maxTotalWidth: maxTotalWidth);

        Debug.WriteLine(formatted);

        if (formatted.EndsWith(Environment.NewLine)) {
            Console.Write(formatted);
        }
        else {
            Console.WriteLine(formatted);
        }

        return source;
    }

    public static IReadOnlyList<T> Print<T, TSelect>(this IEnumerable<T> items,
                                                     [NotNull] Func<T, TSelect> select,
                                                     Func<TSelect, bool> where = null,
                                                     int limit = _defaultItemLimit) {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        list.PrintHeadTail(i => i.Print(), select, where, limit);

        return list;
    }

    public static IReadOnlyList<T> Print<T>(this IEnumerable<T> items,
                                            Func<T, bool> where = null,
                                            int limit = _defaultItemLimit) {
        return items.PrintHeadTail(i => Console.WriteLine(i), where, limit);
    }

    public static object PrintLiteral(this string source, string label = null, int? labelWidth = null, int maxTotalWidth = _defaultDisplayWidth) {
        return Print(source, label, labelWidth, literal: true, maxTotalWidth);
    }

    public static IReadOnlyList<T> PrintLiteral<T, TSelect>(this IEnumerable<T> items,
                                                            [NotNull] Func<T, TSelect> select,
                                                            Func<TSelect, bool> where = null,
                                                            int limit = _defaultItemLimit) {
        return items.PrintHeadTail(i => i.ToCSharpLiteral().Print(), select, where, limit);
    }

    public static IReadOnlyList<T> PrintLiteral<T>(this IEnumerable<T> items,
                                                   Func<T, bool> where = null,
                                                   int limit = _defaultItemLimit) {
        return items.PrintHeadTail(i => i.ToCSharpLiteral().Print(), where, limit);
    }

    public static IEnumerable<T> PrintMessage<T>(this IEnumerable<T> items, string message) {
        message.Print();
        PrintEndSeparator();

        return items;
    }
}

public static partial class PrintExtension {
    public static void PrintJson(this object subject, Formatting format = Formatting.Indented) {
        FormatJson(subject, format: format).Print();
    }

    public static IReadOnlyList<TSelect> PrintJson<T, TSelect>(this IEnumerable<T> items,
                                                               [NotNull] Func<T, TSelect> select,
                                                               Func<TSelect, bool> where = null,
                                                               int limit = _defaultItemLimit) {
        return items.Select(select)
                    .PrintJson(limit: limit, where: where);
    }

    public static IReadOnlyList<T> PrintJson<T>(this IEnumerable<T> items,
                                                Func<T, bool> where = null,
                                                Formatting format = Formatting.Indented,
                                                int limit = _defaultItemLimit) {
        IReadOnlyList<T> list = where == null
                                    ? items.ToReadOnlyList()
                                    : items.Where(where).ToReadOnlyList();


        FormatJson(list.Take(limit), format: format).Print();

        if (list.Count > limit) {
            $" ... {list.Count - limit:N0} more ...".Print();
        }

        PrintEndSeparator();

        return list;
    }

    public static IReadOnlyList<TSelect> PrintJsonLine<T, TSelect>(this IEnumerable<T> items,
                                                                   [NotNull] Func<T, TSelect> select,
                                                                   Func<TSelect, bool> where = null,
                                                                   int limit = _defaultItemLimit) {
        return items.Select(select)
                    .PrintJsonLine(limit: limit, where: where);
    }

    public static IReadOnlyList<T> PrintJsonLine<T>(this IEnumerable<T> items,
                                                    Func<T, bool> where = null,
                                                    int limit = _defaultItemLimit) {
        return items.PrintJson(where, Formatting.None, limit);
    }
}

public static partial class PrintExtension {
    public static IReadOnlyList<T> PrintCount<T>(this IEnumerable<T> items, string label = null) {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Count.ToString("N0").Print(label);
        PrintEndSeparator();

        return list;
    }

    public static IReadOnlyList<T> PrintDistinctCount<T>(this IEnumerable<T> items, string label = null) {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Distinct().Count().ToString("N0").Print(label);
        PrintEndSeparator();

        return list;
    }
}

public static partial class PrintExtension {
    private static IReadOnlyList<T> PrintHeadTail<T>(this IEnumerable<T> items,
                                                     Action<T> output,
                                                     Func<T, bool> where = null,
                                                     int limit = _defaultItemLimit) {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        int headCount = list.Count;
        var tailCount = 0;

        if (list.Count > limit) {
            double size = limit / 2d;
            headCount = Convert.ToInt32(Math.Ceiling(size));
            tailCount = Convert.ToInt32(Math.Floor(size));
        }

        List<T> sources = where == null
                              ? list.ToList()
                              : list.Where(where).ToList();

        sources.Head(headCount).ForEach(output);

        if (tailCount > 0) {
            if (sources.Count > limit) {
                $" ... {sources.Count - limit:N0} skipped ...".Print();
            }

            sources.Tail(tailCount).ForEach(output);
        }

        PrintEndSeparator();

        return list;
    }

    private static IReadOnlyList<T> PrintHeadTail<T, TSelect>(this IEnumerable<T> items,
                                                              Action<TSelect> output,
                                                              [NotNull] Func<T, TSelect> select,
                                                              Func<TSelect, bool> where = null,
                                                              int limit = _defaultItemLimit) {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        list.Select(select)
            .PrintHeadTail(output, where, limit);

        return list;
    }
}

public static partial class PrintExtension {
    private static string CallerName() {
        string ret = null;
        var trace = new StackTrace(0, true);

        for (var i = 0; i <= trace.FrameCount - 1; i++) {
            StackFrame frame = trace.GetFrame(i);
            string filename = frame.GetFileName();
            if (filename?.Contains("Print.Extension") == true) continue;

            MethodBase method = frame.GetMethod();

            ret = $"{method.DeclaringType?.Name}.{method.Name}():{frame.GetFileLineNumber(),-4}";
            break;
        }

        return ret;
    }

    private static void PrintEndSeparator() {
        Console.WriteLine($"--- {CallerName(),_defaultDisplayWidth - 3}");
    }
}

public static partial class PrintExtension {
    private static string Format(object source,
                                 string label = null,
                                 int? labelWidth = null,
                                 int maxTotalWidth = _defaultDisplayWidth) {
        string newline = Environment.NewLine;
        var output = source?.ToString();

        string callerName = CallerName();
        output ??= "<null>";

        if (label == null) {
            return AppendCallerName(output);
        }

        if (labelWidth != null) {
            label = label.PadRight(labelWidth.Value);
        }

        if (output.Contains(newline) == false) {
            int totalWidth = Math.Max(label.Length + 1, labelWidth ?? 0) + output.Length;
            if (totalWidth < maxTotalWidth) {
                return AppendCallerName($"{label}: {output}");
            }
        }

        string indent = new(' ', 4);
        output = output.Replace(newline, $"{newline}{indent}");

        return $"{label}:{newline}{indent}{AppendCallerName(output)}";

        string AppendCallerName(string localOutput) {
            return !localOutput.Contains(newline)
                       ? $"{localOutput}{callerName.PadLeft(Math.Max(0, maxTotalWidth - localOutput.Length))}"
                       : $"{localOutput.TrimEnd('\r', '\n')}{newline}--- {callerName.PadLeft(Math.Max(0, maxTotalWidth - 4))}";
        }
    }

    private static string FormatJson(object source,
                                     string label = null,
                                     int? labelWidth = null,
                                     Formatting format = Formatting.Indented,
                                     int maxTotalWidth = 100) {
        string output = source is IToJson json
                            ? json.ToJson().ToString(format)
                            : JsonConvert.SerializeObject(source, format);

        return Format(output, label, labelWidth, maxTotalWidth);
    }
}

public interface IToJson {
    JObject ToJson();
}