namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

public static partial class Extension
{
    public static bool IsNumeric(this object subject)
    {
        return subject is long or int or uint or byte or double or float or decimal;
    }

    public static string ToCSharpLiteral(this double subject)
    {
        return $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}d";
    }

    public static string ToCSharpLiteral(this decimal subject)
    {
        return $"{subject.ToString("#,##0.#####################################################", CultureInfo.InvariantCulture).Replace(",", "_")}m";
    }

    public static string ToCSharpLiteral(this int subject)
    {
        return subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_");
    }

    public static string ToCSharpLiteral(this long subject)
    {
        return subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_");
    }

    public static string ToCSharpLiteral(this object subject, bool strict = false)
    {
        if (strict && subject is DateTime date) return $"DateTime.Parse(\"{date.ToLiteral()}\")";

        return subject switch {
                   null            => "<null>",
                   int literal     => literal.ToCSharpLiteral(),
                   long literal    => literal.ToCSharpLiteral(),
                   double literal  => literal.ToCSharpLiteral(),
                   decimal literal => literal.ToCSharpLiteral(),
                   _               => subject.ToLiteral()
               };
    }

    public static string ToLiteral(this object obj)
    {
        if (obj is DateTime dt)
        {
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

public static partial class Extension
{
    public static string RenderNumbered(this IEnumerable<object> items, int startNumber = 1)
    {
        if (items == null) return null;

        items = items.ToReadOnlyList();
        if (!items.Any()) return "<none>";

        int maxPositionLength = items.Select((_, idx) => (idx + startNumber).ToString().Length).Max();
        int indent = maxPositionLength + 2;
        IEnumerable<string> lines = items.Select((item, idx) => {
                                                     int position = idx + startNumber;
                                                     string num = maxPositionLength > 1
                                                                      ? position.ToString().PadLeft(maxPositionLength)
                                                                      : position.ToString();

                                                     return $"{num}: {item.ToString().IndentSkipFirstLine(indent)}";
                                                 });

        return lines.ToJoinNewlineString();
    }
}