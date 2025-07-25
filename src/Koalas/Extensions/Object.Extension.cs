namespace Koalas.Extensions;

#nullable enable
using System.Security.Cryptography;

public static partial class ObjectExtension
{
    /// <summary>
    ///     Renders the subject as a string suitable to print in a human-readable form.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this object? subject)
    {
        return subject switch {
                   null          => "null",
                   string item   => item.Render(),
                   DateTime item => item.Render(),
                   int item      => item.Render(),
                   uint item     => item.Render(),
                   decimal item  => item.Render(),
                   double item   => item.Render(),
                   float item    => item.Render(),
                   IRender item  => item.Render(),
                   IToJson item  => item.ToJson().ToString(Formatting.Indented),
                   _             => subject.ToLiteral()
               };
    }

    public static string Render(this string? subject)
    {
        return subject == null
                   ? "null"
                   : subject.Contains("\n")
                       ? $"\"\"\"{Environment.NewLine}{subject}{Environment.NewLine}\"\"\""
                       : subject.ToLiteral().Replace(@"\\", @"\");
    }

    public static string Render(this DateTime subject)
    {
        return subject.ToLiteral();
    }

    public static string Render(this int subject)
    {
        return subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_");
    }

    public static string Render(this uint subject)
    {
        return $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}u";
    }

    public static string Render(this float subject)
    {
        return $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}f";
    }

    public static string Render(this double subject)
    {
        return $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}d";
    }

    public static string Render(this decimal subject)
    {
        return $"{subject.ToString("#,##0.#####################################################", CultureInfo.InvariantCulture).Replace(",", "_")}m";
    }
}

public static partial class ObjectExtension
{
    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this object? subject)
    {
        return subject switch {
                   string item   => item.ToCSharpLiteral(),
                   DateTime item => item.ToCSharpLiteral(),
                   _             => subject.Render()
               };
    }

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this string? subject)
    {
        return subject.ToLiteral();
    }

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this DateTime subject)
    {
        return $"DateTime.Parse(\"{subject.ToLiteral()}\")";
    }
}

public static partial class ObjectExtension
{
    public static string ToJsonHash(this object request)
    {
        string requestJson = JsonConvert.SerializeObject(request, Formatting.None)
                                        .Replace("\r\n", "\n")
                                        .Replace("\r", "\n")
                                        .Replace("\\r\\n", "\\n");

        byte[] inputBytes = Encoding.UTF8.GetBytes(requestJson);

        StringBuilder output = new();
        SHA256.Create()
              .ComputeHash(inputBytes)
              .ForEach(b => output.Append(b.ToString("X2")));

        return output.ToString();
    }

    public static string ToJsonLineString(this object source, params JsonConverter[] converters)
    {
        return ToJsonString(source, Formatting.None, converters);
    }

    public static string ToJsonLineString(this object source)
    {
        return ToJsonString(source, Formatting.None);
    }

    public static string ToJsonString(this object source, JsonSerializerSettings? settings = null)
    {
        return ToJsonString(source, Formatting.None, settings);
    }

    public static string ToJsonString(this object source, params JsonConverter[] converters)
    {
        return ToJsonString(source, Formatting.Indented, converters);
    }

    public static string ToJsonString(this object source, Formatting formatting = Formatting.Indented, params JsonConverter[] converters)
    {
        return JsonConvert.SerializeObject(source, formatting, converters);
    }

    public static string ToJsonString(this object source, Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.SerializeObject(source, formatting, settings);
    }
}

public static partial class ObjectExtension
{
    public static bool SequenceValueEquals<T>(this IEnumerable<T>? left, IEnumerable<T>? right)
    {
        return (left == null && right == null)
               || (right != null && left?.SequenceEqual(right) == true);
    }

    public static bool ValueEquals<T>(this T? left, T? right)
    {
        return (left == null && right == null)
               || (right != null && left?.Equals(right) == true);
    }
}

public static partial class Extension
{
    public static string ToLiteral(this object? obj)
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
    public static string RenderNumbered(this IEnumerable<object>? items, int startNumber = 1)
    {
        if (items == null) return string.Empty;

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