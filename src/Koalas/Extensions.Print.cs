namespace Koalas;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using static Extensions;

public static partial class Extensions {
    public static IReadOnlyList<T> Print<T>(this IEnumerable<T> items,
                                            Func<T, object> selector = null,
                                            int tail = 5) {
        var list = items.CoerceList();
        selector ??= i => i;

        var listTotalCount = list.Count;
        if (listTotalCount > tail) {
            $"[skipped {listTotalCount - tail:N0}]".Print();
        }

        foreach (var content in list.Tail(tail).Select(selector).ToList()) {
            content.Print();
        }

        "--".Print();

        return list;
    }

    public static object Print(this string source, string label = null, int? labelWidth = null, int maxTotalWidth = 100) {
        return Print((object)source, label, labelWidth, maxTotalWidth);
    }

    public static object Print(this object source, string label = null, int? labelWidth = null, int maxTotalWidth = 100) {
        string formatted = source.Format(label, labelWidth, json: false, literal: false, maxTotalWidth: maxTotalWidth);
        //Debug.WriteLine(formatted);

        if (formatted.EndsWith(Environment.NewLine)) {
            Console.Write(formatted);
        }
        else {
            Console.WriteLine(formatted);
        }

        return source;
    }

    public static IReadOnlyList<T> PrintCount<T>(this IEnumerable<T> items, string label = null) {
        var list = items.CoerceList();
        list.Count.ToString("N0").Print(label);
        "--".Print();

        return list;
    }

    public static IReadOnlyList<T> PrintDistinctCount<T>(this IEnumerable<T> items, string label = null) {
        var list = items.CoerceList();
        list.Distinct().Count().ToString("N0").Print(label);
        "--".Print();

        return list;
    }

    public static IReadOnlyList<T> PrintJson<T>(this IEnumerable<T> items,
                                                Func<T, object> selector = null,
                                                Formatting format = Formatting.Indented,
                                                int tail = 5) {
        var list = items.CoerceList();
        selector ??= i => i;
        var listTotalCount = list.Count;
        if (listTotalCount > tail) {
            $"[skipped {listTotalCount - tail:N0}]".Print();
        }

        list.Tail(tail)
            .Select(selector)
            .Select(item => Format(item, json: true, format: format))
            .Print(tail: tail);

        return list;
    }

    public static IReadOnlyList<T> PrintJsonLine<T>(this IEnumerable<T> items,
                                                    Func<T, object> selector = null,
                                                    int tail = 5) {
        return items.PrintJson(selector, Formatting.None, tail);
    }

    public static IReadOnlyList<T> PrintLiteral<T>(this IEnumerable<T> items,
                                                   Func<T, object> selector = null,
                                                   int tail = 5) {
        var list = items.CoerceList();
        selector ??= i => i;
        list.Print(i => Format(selector(i), literal: true), tail: tail);

        return list;
    }

    public static IEnumerable<T> PrintMessage<T>(this IEnumerable<T> items, string message) {
        Console.WriteLine(message);
        "--".Print();

        return items;
    }

    public static string ToCSharpCodeLiteral(this object obj) {
        return obj switch {
                   null            => "null",
                   DateTime date   => $"DateTime.Parse(\"{date.ToLiteral()}\")",
                   double literal  => literal.ToCSharpLiteral(),
                   decimal literal => literal.ToCSharpLiteral(),
                   _               => obj.ToLiteral()
               };
    }

    public static string ToCSharpLiteral(this double source) {
        return $"{source}d";
    }

    public static string ToCSharpLiteral(this decimal source) {
        return $"{source}m";
    }

    public static string ToCSharpLiteral(this object obj) {
        return obj switch {
                   null            => "<null>",
                   double literal  => literal.ToCSharpLiteral(),
                   decimal literal => literal.ToCSharpLiteral(),
                   _               => obj.ToLiteral()
               };
    }

    public static string ToLiteral(this object obj) {
        if (obj is DateTime dt) {
            return dt.ToString(dt == dt.Date
                                   ? "yyyy-MM-dd"
                                   : dt.Second == 0 && dt.Millisecond == 0
                                       ? "yyyy-MM-ddTHH:mm"
                                       : dt.Millisecond == 0
                                           ? "s"
                                           : "yyyy-MM-ddTHH:mm:ss.fff",
                               CultureInfo.InvariantCulture);
        }

        return JsonConvert.SerializeObject(obj);
    }

    private static string Format(this object source,
                                 string label = null,
                                 int? labelWidth = null,
                                 bool literal = false,
                                 bool json = false,
                                 Formatting format = Formatting.Indented,
                                 int maxTotalWidth = 100) {
        string output;

        if (json) {
            output = JsonConvert.SerializeObject(source, format);
        }
        else if (literal) {
            output = source switch {
                         int i      => i.ToCSharpLiteral(),
                         double d   => d.ToCSharpLiteral(),
                         decimal d  => d.ToCSharpLiteral(),
                         DateTime d => d.ToCSharpLiteral(),
                         _          => source?.ToString()
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

        string newline = Environment.NewLine;

        if (output.Contains(newline) == false) {
            var totalWidth = Math.Max(label.Length + 1, labelWidth ?? 0) + output.Length;
            if (totalWidth < maxTotalWidth) {
                return $"{label}: {output}";
            }
        }

        string indent = new(' ', 4);
        output = output.Replace(newline, $"{newline}{indent}");

        return $"{label}:{newline}{indent}{output}";
    }
}
