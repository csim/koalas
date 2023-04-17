namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static partial class Extension {
    public static object Print(this string source, string label = null, int? labelWidth = null, int maxTotalWidth = 100) {
        return Print((object) source, label, labelWidth, maxTotalWidth);
    }

    public static object Print(this object source, string label = null, int? labelWidth = null, int maxTotalWidth = 100) {
        string formatted = source.Format(label, labelWidth, maxTotalWidth: maxTotalWidth);
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
                                                     Func<T, TSelect> select,
                                                     Func<TSelect, bool> where = null,
                                                     int limit = 100) {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        list.PrintHeadTail(i => i.Print(), select, where, limit);

        return list;
    }

    public static IReadOnlyList<T> Print<T>(this IEnumerable<T> items,
                                            Func<T, bool> where = null,
                                            int limit = 100) {
        return items.PrintHeadTail(i => i.Print(), where, limit);
    }

    public static IReadOnlyList<T> PrintLiteral<T, TSelect>(this IEnumerable<T> items,
                                                            Func<T, TSelect> select,
                                                            Func<TSelect, bool> where = null,
                                                            int limit = 100) {
        return items.PrintHeadTail(i => i.ToCSharpLiteral().Print(), select, where, limit);
    }

    public static IReadOnlyList<T> PrintLiteral<T>(this IEnumerable<T> items,
                                                   Func<T, bool> where = null,
                                                   int limit = 100) {
        return items.PrintHeadTail(i => i.ToCSharpLiteral().Print(), where, limit);
    }

    public static IEnumerable<T> PrintMessage<T>(this IEnumerable<T> items, string message) {
        message.Print();
        "---".Print();

        return items;
    }
}

public static partial class Extension {
    public static IReadOnlyList<TSelect> PrintJson<T, TSelect>(this IEnumerable<T> items,
                                                               Func<T, TSelect> select,
                                                               Func<TSelect, bool> where = null,
                                                               int limit = 100) {
        return items.Select(select)
                    .PrintJson(limit: limit, where: where);
    }

    public static IReadOnlyList<T> PrintJson<T>(this IEnumerable<T> items,
                                                Func<T, bool> where = null,
                                                Formatting format = Formatting.Indented,
                                                int limit = 100) {
        IReadOnlyList<T> list = where == null 
                                    ? items.ToReadOnlyList()
                                    : items.Where(where).ToReadOnlyList();

        list.Take(limit)
            .FormatJson(format: format)
            .Print();

        if (list.Count > limit) {
            $" ... {list.Count - limit:N0} more ...".Print();
        }

        "---".Print();

        return list;
    }

    public static IReadOnlyList<TSelect> PrintJsonLine<T, TSelect>(this IEnumerable<T> items,
                                                                   Func<T, TSelect> select,
                                                                   Func<TSelect, bool> where = null,
                                                                   int limit = 100) {
        return items.Select(select)
                    .PrintJsonLine(limit: limit, where: where);
    }

    public static IReadOnlyList<T> PrintJsonLine<T>(this IEnumerable<T> items,
                                                    Func<T, bool> where = null,
                                                    int limit = 100) {
        return items.PrintJson(where, Formatting.None, limit);
    }
}

public static partial class Extension {
    public static IReadOnlyList<T> PrintCount<T>(this IEnumerable<T> items, string label = null) {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Count.ToString("N0").Print(label);
        "---".Print();

        return list;
    }

    public static IReadOnlyList<T> PrintDistinctCount<T>(this IEnumerable<T> items, string label = null) {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        list.Distinct().Count().ToString("N0").Print(label);
        "---".Print();

        return list;
    }
}

public static partial class Extension {
    private static IReadOnlyList<T> PrintHeadTail<T>(this IEnumerable<T> items,
                                                     Action<T> output,
                                                     Func<T, bool> where = null,
                                                     int limit = 100) {
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

        "---".Print();

        return list;
    }

    private static IReadOnlyList<T> PrintHeadTail<T, TSelect>(this IEnumerable<T> items,
                                                              Action<TSelect> output,
                                                              Func<T, TSelect> select,
                                                              Func<TSelect, bool> where = null,
                                                              int limit = 100) {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        list.Select(select)
            .PrintHeadTail(output, where, limit);

        return list;
    }
}

public static partial class Extension {
    private static string Format(this object source,
                                 string label = null,
                                 int? labelWidth = null,
                                 bool literal = false,
                                 int maxTotalWidth = 100) {
        string output;

        if (literal) {
            output = source switch {
                         int i      => i.ToCSharpLiteral(),
                         double d   => d.ToCSharpLiteral(),
                         decimal d  => d.ToCSharpLiteral(),
                         DateTime d => d.ToCSharpLiteral(),
                         _          => source?.ToLiteral()
                     };
        }
        else {
            output = source?.ToString();
        }

        output ??= "<null>";

        if (label == null) {
            return output;
        }

        if (labelWidth != null) {
            label = label.PadRight(labelWidth.Value);
        }

        if (output.Contains(_newline) == false) {
            int totalWidth = Math.Max(label.Length + 1, labelWidth ?? 0) + output.Length;
            if (totalWidth < maxTotalWidth) {
                return $"{label}: {output}";
            }
        }

        string indent = new(' ', 4);
        output = output.Replace(_newline, $"{_newline}{indent}");

        return $"{label}:{_newline}{indent}{output}";
    }

    private static string FormatJson(this object source,
                                     string label = null,
                                     int? labelWidth = null,
                                     Formatting format = Formatting.Indented,
                                     int maxTotalWidth = 100) {
        string output = source is IToJson json
                            ? json.ToJson().ToString(format)
                            : JsonConvert.SerializeObject(source, format);

        return output.Format(label, labelWidth, false, maxTotalWidth);
    }
}

public interface IToJson {
    JObject ToJson();
}