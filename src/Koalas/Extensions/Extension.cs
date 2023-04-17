namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

public static partial class Extension {
    private static readonly string _newline = Environment.NewLine;
}

public static partial class Extension {
    public static string ToLiteral(this object obj) {
        if (obj is DateTime dt) {
            return dt.ToString(dt == dt.Date
                                   ? "yyyy-MM-dd"
                                   : dt is { Second: 0, Millisecond: 0 }
                                       ? "yyyy-MM-ddTHH:mm"
                                       : dt.Millisecond == 0
                                           ? "s"
                                           : "yyyy-MM-ddTHH:mm:ss.fff",
                               CultureInfo.InvariantCulture);
        }

        return JsonConvert.SerializeObject(obj);
    }
}

public static partial class Extension {
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
        return $"{source.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}d";
    }

    public static string ToCSharpLiteral(this decimal source) {
        return $"{source.ToString("#,##0.#####################################################", CultureInfo.InvariantCulture).Replace(",", "_")}m";
    }

    public static string ToCSharpLiteral(this object obj) {
        return obj switch {
                   null            => "<null>",
                   double literal  => literal.ToCSharpLiteral(),
                   decimal literal => literal.ToCSharpLiteral(),
                   _               => obj.ToLiteral()
               };
    }
}

public static partial class Extension {
    public static string RenderNumbered(this IEnumerable<object> items, int startNumber = 1) {
        if (items == null) {
            return null;
        }

        items = items.ToReadOnlyList();
        if (!items.Any()) {
            return "<none>";
        }

        int maxPositionLength = items.Select((_, idx) => (idx + startNumber).ToString().Length).Max();
        int indent = maxPositionLength + 2;
        IEnumerable<string> lines = items.Select((item, idx) => {
                                                     int position = idx + startNumber;
                                                     string num = maxPositionLength > 1
                                                                      ? position.ToString().PadLeft(maxPositionLength)
                                                                      : position.ToString();

                                                     return $"{num}: {item.ToString().IndentSkipFirstLine(indent)}";
                                                 });

        return string.Join(_newline, lines);
    }
}