using System.Security.Cryptography;

namespace Outback.Extensions;

public static class ObjectExtensions
{
    private static readonly JsonSerializerOptions _defaultCompactOptions = new()
    {
        WriteIndented = false,
    };
    private static readonly JsonSerializerOptions _defaultIndentedOptions = new()
    {
        WriteIndented = true,
    };

    public static bool HasClassAttribute<T>(this object? subject)
        where T : Attribute
    {
        return subject != null && subject.GetType().IsDefined(typeof(T), inherit: true);
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this object? subject)
    {
        if (subject == null)
        {
            return "null";
        }

        string? ret = subject switch
        {
            string item => item.Render(),
            bool item => item ? "true" : "false",
            int item => item.Render(),
            uint item => item.Render(),
            long item => item.Render(),
            ulong item => item.Render(),
            float item => item.Render(),
            double item => item.Render(),
            decimal item => item.Render(),
            DateTime item => item.Render(),
            IDictionary<string, string> item => item.Render(),
            IDictionary<string, object> item => item.Render(),
            IDictionary<object, string> item => item.Render(),
            _ => null,
        };

        return ret
            ?? (
                subject is IRender renderSubject ? renderSubject.Render()
                : subject is IToJson
                    ? $"""
                        {subject.GetType().Name}
                        {subject.ToJsonString()}
                        """
                : subject.ToString()
            );
    }

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this string? subject)
    {
        return subject == null ? string.Empty
            : subject.Contains('\n') || subject.Contains('"')
                ? $"\"\"\"{Environment.NewLine}{subject}{Environment.NewLine}\"\"\""
            : subject.ToString();
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this DateTime subject)
    {
        string mask =
            subject == subject.Date ? "yyyy-MM-dd"
            : subject is { Second: 0, Millisecond: 0 } ? "yyyy-MM-ddTHH:mm"
            : subject.Millisecond == 0 ? "yyyy-MM-ddTHH:mm:ss"
            : "yyyy-MM-ddTHH:mm:ss.fff";

        return subject.ToString(mask, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this int subject)
    {
        return subject
            .ToString("#,##0.#####################", CultureInfo.InvariantCulture)
            .Replace(",", "_");
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this uint subject)
    {
        return $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}u";
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this long subject)
    {
        return subject
            .ToString("#,##0.#####################", CultureInfo.InvariantCulture)
            .Replace(",", "_");
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this ulong subject)
    {
        return subject
            .ToString("#,##0.#####################", CultureInfo.InvariantCulture)
            .Replace(",", "_");
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this float subject)
    {
        return $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}f";
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this double subject)
    {
        return $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}d";
    }

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this decimal subject)
    {
        return $"{subject.ToString("#,##0.#####################################################", CultureInfo.InvariantCulture).Replace(",", "_")}m";
    }

    public static bool SequenceValueEquals<T>(this IEnumerable<T>? left, IEnumerable<T>? right)
    {
        return (left == null && right == null)
            || (right != null && left?.SequenceEqual(right) == true);
    }

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this DateTime subject)
    {
        return $"DateTime.Parse(\"{subject.Render()}\")";
    }

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this object? subject, bool allowVerbatim = true)
    {
        return subject switch
        {
            null => "null",
            string item => item.ToCSharpLiteral(allowVerbatim: allowVerbatim),
            bool item => item ? "true" : "false",
            int item => item.Render(),
            long item => item.Render(),
            ulong item => item.Render(),
            uint item => item.Render(),
            double item => item.Render(),
            float item => item.Render(),
            decimal item => item.Render(),
            DateTime item => item.ToCSharpLiteral(),
            _ => subject.ToString(),
        };
    }

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this string? subject, bool allowVerbatim = true)
    {
        return subject == null ? "null"
            : allowVerbatim && (subject.Contains('\n') || subject.Contains('"'))
                ? $"\"\"\"{Environment.NewLine}{subject}{Environment.NewLine}\"\"\""
            : subject.ToString();
    }

    public static string ToHash(this string? source)
    {
        if (source == null)
        {
            return string.Empty;
        }

        // Replacements necessary for cross-platform compatibility
        string cleanSource = source
            .Replace(@"\r\n", @"\n")
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        byte[] inputBytes = Encoding.UTF8.GetBytes(cleanSource);

        StringBuilder output = new();
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(inputBytes);
        foreach (byte c in hash)
        {
            output.Append(c.ToString("X2"));
        }

        return output.ToString();
    }

    public static string ToJsonHash(this object? source)
    {
        return source?.ToJsonLineString().ToHash() ?? string.Empty;
    }

    public static string ToJsonLineString(this object source, JsonSerializerOptions? options = null)
    {
        return ToJsonString(source, indented: false, options);
    }

    public static string ToJsonString(this object source)
    {
        return ToJsonString(source, indented: true);
    }

    public static string ToJsonString(this object source, JsonSerializerOptions? options)
    {
        return ToJsonString(source, indented: true, options);
    }

    public static string ToJsonString(this JsonNode? node, bool indented = true)
    {
        JsonSerializerOptions options = indented ? _defaultIndentedOptions : _defaultCompactOptions;
        return node?.ToJsonString(options) ?? "null";
    }

    public static string ToJsonString(
        this object source,
        bool indented,
        JsonSerializerOptions? options = null
    )
    {
        object? serializationSource = source;
        if (source is IToJson sourceJson)
        {
            serializationSource = sourceJson.ToJson();
        }

        JsonSerializerOptions serializerOptions =
            options ?? (indented ? _defaultIndentedOptions : _defaultCompactOptions);

        return JsonSerializer.Serialize(serializationSource, serializerOptions);
    }

    public static bool ValueEquals<T>(this T? left, T? right)
    {
        return (left == null && right == null) || (right != null && left?.Equals(right) == true);
    }

    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }
}
