namespace Koalas;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using static Extensions;

public static partial class Extensions {
    public static IReadOnlyList<T> Print<T, TSelect>(this IEnumerable<T> items,
                                                     int limit = 6,
                                                     Func<T, TSelect> select = null,
                                                     Func<T, bool> where = null) {
        return items.PrintHeadTail(limit, output: i => i.Print(), where: where, select: select);
    }

    public static IReadOnlyList<T> Print<T>(this IEnumerable<T> items,
                                            int limit = 6,
                                            Func<T, bool> where = null) {
        return items.PrintHeadTail(limit, output: i => i.Print(), where: where);
    }

    public static IReadOnlyList<T> PrintJson<T, TSelect>(this IEnumerable<T> items,
                                                         int limit = 6,
                                                         Func<T, TSelect> select = null,
                                                         Func<T, bool> where = null,
                                                         Formatting format = Formatting.Indented) {
        return items.PrintHeadTail(limit, output: i => i.Format(json: true, format: format).Print(), where: where, select: select);
    }

    public static IReadOnlyList<T> PrintJson<T>(this IEnumerable<T> items,
                                                int limit = 6,
                                                Func<T, bool> where = null,
                                                Formatting format = Formatting.Indented) {
        return items.PrintHeadTail(limit, output: i => i.Format(json: true, format: format).Print(), where: where);
    }

    public static IReadOnlyList<T> PrintJsonLine<T, TSelect>(this IEnumerable<T> items,
                                                             int limit = 6,
                                                             Func<T, TSelect> select = null,
                                                             Func<T, bool> where = null) {
        return items.PrintHeadTail(limit, output: i => i.Format(json: true, format: Formatting.None).Print(), where: where, select: select);
    }

    public static IReadOnlyList<T> PrintJsonLine<T>(this IEnumerable<T> items,
                                                    int limit = 6,
                                                    Func<T, bool> where = null) {
        return items.PrintHeadTail(limit, output: i => i.Format(json: true, format: Formatting.None).Print(), where: where);
    }

    public static IReadOnlyList<T> PrintLiteral<T, TSelect>(this IEnumerable<T> items,
                                                            int limit = 6,
                                                            Func<T, TSelect> select = null,
                                                            Func<T, bool> where = null) {
        return items.PrintHeadTail(limit, output: i => i.ToCSharpLiteral().Print(), where: where, select: select);
    }

    public static IReadOnlyList<T> PrintLiteral<T>(this IEnumerable<T> items,
                                                   int limit = 6,
                                                   Func<T, bool> where = null) {
        return items.PrintHeadTail(limit, output: i => i.ToCSharpLiteral().Print(), where: where);
    }

    public static IEnumerable<T> PrintMessage<T>(this IEnumerable<T> items, string message) {
        message.Print();
        "--".Print();

        return items;
    }

    private static IReadOnlyList<T> PrintHeadTail<T>(this IEnumerable<T> items,
                                                     int limit,
                                                     Action<T> output,
                                                     Func<T, bool> where = null) {
        var size = limit / 2d;
        var head = Convert.ToInt32(Math.Floor(size));
        var tail = Convert.ToInt32(Math.Ceiling(size));

        where ??= _ => true;
        var list = items.CoerceList();
        var sources = list.Where(where).ToList();

        sources.Head(head)
               .ForAll(i => output(i));

        if (head < sources.Count) {
            if (sources.Count > limit) $"[skipped {sources.Count - limit}]".Print();

            sources.Tail(tail)
                   .ForAll(i => output(i));
        }

        "--".Print();

        return list;
    }

    private static IReadOnlyList<T> PrintHeadTail<T, TSelect>(this IEnumerable<T> items,
                                                              int limit,
                                                              Action<TSelect> output,
                                                              Func<T, TSelect> select,
                                                              Func<T, bool> where = null) {
        var size = limit / 2d;
        var head = Convert.ToInt32(Math.Floor(size));
        var tail = Convert.ToInt32(Math.Ceiling(size));

        where ??= _ => true;
        var list = items.CoerceList();
        var sources = list.Where(where)
                          .Select(select)
                          .ToList();

        sources.Head(head)
               .ForAll(i => output(i));

        if (head < sources.Count) {
            if (sources.Count > limit) $"[skipped {sources.Count - limit}]".Print();

            sources.Tail(tail)
                   .ForAll(i => output(i));
        }

        "--".Print();

        return list;
    }
}

public static partial class Extensions {
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

        if (true) {
            var x = 1;
        }

        if (json) {
            output = JsonConvert.SerializeObject(source, format);
        }
        else if (literal) {
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
