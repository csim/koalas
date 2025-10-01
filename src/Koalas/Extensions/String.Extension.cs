namespace Koalas.Extensions;

public static partial class StringExtensions
{
    // ReSharper disable once InconsistentNaming
    private static readonly Regex _indentRegex = new(
        @"^(\s*)(?:.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline
    );

    public static string After(this string? subject, string findText)
    {
        if (subject == null || string.IsNullOrEmpty(findText))
            return string.Empty;

        int index = subject.IndexOf(findText, StringComparison.Ordinal);
        return index >= 0 ? subject[(index + findText.Length)..] : subject;
    }

    public static string Before(this string? subject, string findText)
    {
        if (subject == null || string.IsNullOrEmpty(findText))
            return string.Empty;

        int index = subject.IndexOf(findText, StringComparison.Ordinal);
        return index >= 0 ? subject[..index] : subject;
    }

    public static string Compress(this string content)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(content).Compress());
    }

    public static string Concat(this string subject, string value)
    {
        return string.Concat(subject, value);
    }

    public static string Concat(this string subject, IEnumerable<string> values)
    {
        return string.Concat(subject, string.Concat(values));
    }

    public static bool ContainsIgnoreCase(this string subject, string target)
    {
        return subject.Contains(target, StringComparison.OrdinalIgnoreCase);
    }

    public static string Decompress(this string base64Content)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64Content).Decompress());
    }

    public static string HangIndent(this string? subject, string? body, string? suffix = null)
    {
        return string.IsNullOrEmpty(subject)
            ? subject ?? string.Empty
            : $"{subject}{body?.IndentSkipFirstLine(subject.Length)}{suffix}";
    }

    public static IEnumerable<string> Indent(
        this IEnumerable<string>? subject,
        int size = 4,
        bool skipFirstLine = false
    )
    {
        return subject == null
            ? []
            : subject.Select((s, index) => skipFirstLine && index == 0 ? s : s.Indent(size));
    }

    public static string Indent(this string? subject, int size = 4, bool skipFirstLine = false)
    {
        if (string.IsNullOrEmpty(subject) || size == 0)
            return subject ?? string.Empty;

        string indent = new(' ', size);

        return subject
            .Lines()
            .Select((line, index) => index == 0 && skipFirstLine ? line : $"{indent}{line}")
            .ToJoinNewlineString();
    }

    public static IEnumerable<string> IndentSkipFirstLine(
        this IEnumerable<string> subject,
        int length = 4
    )
    {
        return subject.Indent(size: length, skipFirstLine: true);
    }

    public static string IndentSkipFirstLine(this string subject, int size = 4)
    {
        return subject.Indent(size, skipFirstLine: true);
    }

    public static int IndexOfIgnoreCase(this string subject, string target)
    {
        return subject.IndexOf(target, StringComparison.OrdinalIgnoreCase);
    }

    public static int IndexOfOrdinal(this string subject, string target)
    {
        return subject.IndexOf(target, StringComparison.Ordinal);
    }

    public static bool IsNullOrEmpty(this string? subject)
    {
        return string.IsNullOrEmpty(subject);
    }

    public static string LeadingBlankLines(this string? subject, int count)
    {
        return subject == null
            ? string.Empty
            : Environment.NewLine.Repeat(count + 1) + subject.TrimStart('\r', '\n');
    }

    public static IEnumerable<string> Lines(this string? subject)
    {
        if (string.IsNullOrEmpty(subject))
            yield break;

        using StringReader reader = new(subject);

        while (reader.Peek() != -1)
        {
            yield return reader.ReadLine() ?? string.Empty;
        }
    }

    public static string PadBottom(this string subject, int lineCount)
    {
        int actualLineCount = subject.Lines().Count();

        return actualLineCount > lineCount
            ? subject
            : subject + Environment.NewLine.Repeat(lineCount - actualLineCount);
    }

    public static string? PadLinesLeft(this string? subject, int width)
    {
        if (subject == null)
            return null;

        int maxLength = subject.Lines().Max(static l => l.Length);

        return subject.Indent(width - maxLength);
    }

    public static string PadLinesRight(this string? subject, int width)
    {
        return subject?.Lines().Select(l => l.PadRight(width)).ToJoinNewlineString()
            ?? string.Empty;
    }

    public static string PadTop(this string subject, int lineCount)
    {
        int actualLineCount = subject.Lines().Count();

        return actualLineCount > lineCount
            ? subject
            : Environment.NewLine.Repeat(lineCount - actualLineCount) + subject;
    }

    public static T ParseJson<T>(this string source)
    {
        return JsonConvert.DeserializeObject<T>(source)
            ?? throw new InvalidOperationException($"Unable to deserialize: {typeof(T).Name}");
    }

    public static string RemoveTrailingBlankLines(this string? subject)
    {
        return subject?.TrailingBlankLines(0) ?? string.Empty;
    }

    public static string Render(this IEnumerable<string?>? items, string separator = ":")
    {
        if (items == null)
            return string.Empty;

        items = items.ToReadOnlyList();
        return !items.Any()
            ? "<none>"
            : TextBuilder.Create().AddList(items, separator: separator).Render();
    }

    public static string RenderNumbered(
        this IEnumerable<string?> items,
        string separator = ":",
        int startId = 1
    )
    {
        if (startId == 1)
            return items.Render(separator: separator);

        TextListBuilder builder = TextListBuilder.Create().Separator(separator);

        int id = startId;
        foreach (string? item in items)
        {
            builder.AddItem(body: item ?? string.Empty, id: $"{id++}");
        }

        return builder.SaveList().Render();
    }

    public static string Repeat(this char subject, int count)
    {
        return new string(subject, count);
    }

    public static string Repeat(this string subject, int count)
    {
        return string.Concat(Enumerable.Repeat(subject, count));
    }

    public static string Sha256(this string content)
    {
        return Encoding.UTF8.GetBytes(content).Sha256();
    }

    public static string Splice(this string subject, int startIndex, string replacement)
    {
        return Splice(subject, startIndex, replacement, replacement.Length);
    }

    public static string Splice(
        this string subject,
        int startIndex,
        string replacement,
        int removeCount
    )
    {
        return subject.Remove(startIndex, removeCount).Insert(startIndex, replacement);
    }

    public static string StripAllIndent(this string subject)
    {
        return subject.Lines().Select(static l => l.TrimStart()).ToJoinNewlineString();
    }

    public static string StripIndent(this string subject)
    {
        IReadOnlyList<string> indentLines = _indentRegex
            .NonCachingMatches(subject)
            .Select(m => m.Groups[1].Value)
            .ToReadOnlyList();
        if (indentLines.None())
            return subject;

        int minIndent = indentLines.Min(i => i.Length);

        return subject
            .Lines()
            .Select(line => line.Length <= minIndent ? line : line[minIndent..])
            .ToJoinNewlineString();
    }

    public static string ToAnonymizedString(this string? source)
    {
        if (source == null)
            return "<null>";

        IEnumerable<char> chars =
            from c in source
            select CharUnicodeInfo.GetUnicodeCategory(c) switch
            {
                UnicodeCategory.UppercaseLetter => 'A',
                UnicodeCategory.LowercaseLetter => 'a',
                UnicodeCategory.DecimalDigitNumber
                or UnicodeCategory.LetterNumber
                or UnicodeCategory.OtherNumber => '9',
                UnicodeCategory.ClosePunctuation
                or UnicodeCategory.ConnectorPunctuation
                or UnicodeCategory.CurrencySymbol
                or UnicodeCategory.DashPunctuation
                or UnicodeCategory.FinalQuotePunctuation
                or UnicodeCategory.InitialQuotePunctuation
                or UnicodeCategory.LineSeparator
                or UnicodeCategory.MathSymbol
                or UnicodeCategory.ModifierSymbol
                or UnicodeCategory.OpenPunctuation
                or UnicodeCategory.OtherPunctuation
                or UnicodeCategory.OtherSymbol
                or UnicodeCategory.ParagraphSeparator
                or UnicodeCategory.SpaceSeparator => c,
                _ when c is '\r' or '\n' => c,
                _ => 'x',
            };

        string ret = new([.. chars]);

        return ret.Replace("\r", "\\r").Replace("\n", "\\n");
    }

    public static string ToCamelCase(this string str)
    {
        return string.IsNullOrEmpty(str) || str.Length < 2
            ? str
            : char.ToLowerInvariant(str[0]) + str[1..];
    }

    public static string ToValidFilename(this string input, char replacement = '_')
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        char[] result = new char[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            result[i] = Array.IndexOf(invalidChars, c) >= 0 ? replacement : c;
        }

        return new string(result);
    }

    public static string ToWrapString(
        this string? text,
        int? maxLength,
        int overflowIndentSize = 0,
        bool showGlyph = false
    )
    {
        return maxLength == null || text == null || string.IsNullOrEmpty(text)
            ? text ?? string.Empty
            : text.Wrap(
                    maxLength: maxLength.Value,
                    overflowIndentSize: overflowIndentSize,
                    showGlyph: showGlyph
                )
                .ToJoinNewlineString();
    }

    public static string TrailingBlankLines(this string? subject, int count)
    {
        subject ??= string.Empty;

        return subject.TrimEnd('\r', '\n') + Environment.NewLine.Repeat(count + 1);
    }

    public static IEnumerable<string> TrimEnd(this IEnumerable<string>? items)
    {
        return items == null ? [] : items.Select(static i => i.TrimEnd());
    }

    public static string TruncateLines(this string subject, int lineCount)
    {
        string[] lines = [.. subject.Lines()];

        return lines.Length <= lineCount
            ? subject
            : lines.Take(lineCount - 1).ToJoinNewlineString() + Environment.NewLine + "...";
    }

    public static IEnumerable<string> Wrap(
        this string text,
        int maxLength,
        int overflowIndentSize = 0,
        bool showGlyph = false
    )
    {
        List<string> chunks = [];
        const string wrapGlyph = "\u21A9";

        if (string.IsNullOrEmpty(text))
        {
            chunks.Add(string.Empty);

            return chunks;
        }

        // handle newlines
        if (text.Contains('\n'))
        {
            List<string> lines = [.. text.Lines()];
            if (lines.Count > 1)
            {
                List<string> wrappedLines = [];
                foreach (string line in lines)
                {
                    wrappedLines.AddRange(line.Wrap(maxLength, overflowIndentSize, showGlyph));
                }

                return wrappedLines;
            }
        }

        string indent = new(' ', overflowIndentSize);
        int index = -1;

        while (text.Length > 0)
        {
            index++;
            // If remaining string is less than length, add to list and break out of loop
            if (text.Length <= maxLength)
            {
                AddChunk(text, true);
                break;
            }

            // Get maxLength chunk from string.
            string chunk = text[..maxLength];

            // If next char is a space, we can use the whole chunk and remove the space for the next line
            if (char.IsWhiteSpace(text[maxLength]))
            {
                AddChunk(chunk);
                text = text[(chunk.Length + 1)..]; // Remove chunk plus space from original string
            }
            else
            {
                int splitIndex = chunk.LastIndexOf(' '); // Find last space in chunk.

                // If space exists in string,
                if (splitIndex != -1)
                    chunk = chunk[..splitIndex]; // Remove chars after space.

                text = text[(chunk.Length + (splitIndex == -1 ? 0 : 1))..]; // Remove chunk plus space (if found) from original string
                AddChunk(chunk);
            }
        }

        return chunks;

        void AddChunk(string localChuck, bool lastLine = false)
        {
            string localIndent = index > 0 ? indent : string.Empty;
            chunks.Add(
                showGlyph
                    ? $"{localIndent}{localChuck}{(!lastLine ? wrapGlyph : string.Empty)}"
                    : $"{localIndent}{localChuck}"
            );
        }
    }
}
// ReSharper disable once InconsistentNaming
// handle newlines
// If remaining string is less than length, add to list and break out of loop
// Get maxLength chunk from string.
// If next char is a space, we can use the whole chunk and remove the space for the next line
// If space exists in string,
