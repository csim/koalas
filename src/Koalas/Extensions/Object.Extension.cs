using System.Security.Cryptography;

namespace Koalas.Extensions;

public static partial class ObjectExtensions
{
    public static bool HasClassAttribute<T>(this object? subject)
        where T : Attribute =>
        subject != null && subject.GetType().IsDefined(typeof(T), inherit: true);

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
        return ret != null ? ret
            : subject is IRender renderSubject ? renderSubject.Render()
            : subject is IToJson || subject.HasClassAttribute<JsonObjectAttribute>()
                ? $"""
                    {subject.GetType().Name}
                    {subject.ToJsonString()}
                    """
            : ret ?? subject.ToString();
    }

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this string? subject) =>
        subject == null ? string.Empty
        : subject.Contains('\n') || subject.Contains('"')
            ? $"\"\"\"{Environment.NewLine}{subject}{Environment.NewLine}\"\"\""
        : subject.ToString();

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
    public static string Render(this int subject) =>
        subject
            .ToString("#,##0.#####################", CultureInfo.InvariantCulture)
            .Replace(",", "_");

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this uint subject) =>
        $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}u";

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this long subject) =>
        subject
            .ToString("#,##0.#####################", CultureInfo.InvariantCulture)
            .Replace(",", "_");

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this ulong subject) =>
        subject
            .ToString("#,##0.#####################", CultureInfo.InvariantCulture)
            .Replace(",", "_");

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this float subject) =>
        $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}f";

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this double subject) =>
        $"{subject.ToString("#,##0.#####################", CultureInfo.InvariantCulture).Replace(",", "_")}d";

    /// <summary>
    ///     Render <paramref name="subject" /> as a human-readable string.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string Render(this decimal subject) =>
        $"{subject.ToString("#,##0.#####################################################", CultureInfo.InvariantCulture).Replace(",", "_")}m";

    public static bool SequenceValueEquals<T>(this IEnumerable<T>? left, IEnumerable<T>? right) =>
        (left == null && right == null) || (right != null && left?.SequenceEqual(right) == true);

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this object? subject, bool allowVerbatim = true) =>
        subject switch
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

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this string? subject, bool allowVerbatim = true) =>
        subject == null ? "null"
        : allowVerbatim && (subject.Contains('\n') || subject.Contains('"'))
            ? $"\"\"\"{Environment.NewLine}{subject}{Environment.NewLine}\"\"\""
        : subject.ToString();

    /// <summary>
    ///     Renders the subject as a string suitable for use in C# code.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToCSharpLiteral(this DateTime subject) =>
        $"DateTime.Parse(\"{subject.Render()}\")";

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
        var hash = SHA256.Create().ComputeHash(inputBytes);
        foreach (var c in hash)
        {
            output.Append(c.ToString("X2"));
        }

        return output.ToString();
    }

    public static string ToJsonHash(this object? source) =>
        source?.ToJsonLineString().ToHash() ?? string.Empty;

    public static string ToJsonLineString(this object source, params JsonConverter[] converters) =>
        ToJsonString(source, Formatting.None, converters);

    public static string ToJsonLineString(this object source) =>
        ToJsonString(source, Formatting.None);

    public static string ToJsonString(this object source) =>
        ToJsonString(source, Formatting.Indented);

    public static string ToJsonString(this object source, JsonSerializerSettings? settings) =>
        ToJsonString(source, Formatting.Indented, settings);

    public static string ToJsonString(this object source, params JsonConverter[] converters) =>
        ToJsonString(source, Formatting.Indented, converters);

    public static string ToJsonString(
        this object source,
        Formatting formatting,
        params JsonConverter[] converters
    )
    {
        if (source is IToJson sourceJson)
        {
            source = sourceJson.ToJson();
        }

        return JsonConvert.SerializeObject(source, formatting, converters);
    }

    public static string ToJsonString(
        this object source,
        Formatting formatting,
        JsonSerializerSettings? settings
    )
    {
        if (source is IToJson sourceJson)
        {
            source = sourceJson.ToJson();
        }

        return JsonConvert.SerializeObject(source, formatting, settings);
    }

    public static bool ValueEquals<T>(this T? left, T? right) =>
        (left == null && right == null) || (right != null && left?.Equals(right) == true);

    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }
}
