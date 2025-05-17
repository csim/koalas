namespace Koalas.Extensions;

public static partial class StringExtension
{
    private static readonly Regex _indentRegex = new(@"^(\s*)(?:.*)$",
                                                     RegexOptions.Compiled
                                                     | RegexOptions.CultureInvariant
                                                     | RegexOptions.Multiline);

    public static string After(this string subject, string findText)
    {
        if (subject == null || string.IsNullOrEmpty(findText)) return null;

        int index = subject.IndexOf(findText, StringComparison.Ordinal);
        return index >= 0
                   ? subject.Substring(index + findText.Length)
                   : subject;
    }

    public static string Before(this string subject, string findText)
    {
        if (subject == null || string.IsNullOrEmpty(findText)) return null;

        int index = subject.IndexOf(findText, StringComparison.Ordinal);
        return index >= 0
                   ? subject.Substring(0, index)
                   : subject;
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
        return subject.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static int IndexOfIgnoreCase(this string subject, string target)
    {
        return subject.IndexOf(target, StringComparison.OrdinalIgnoreCase);
    }

    public static int IndexOfOrdinal(this string subject, string target)
    {
        return subject.IndexOf(target, StringComparison.Ordinal);
    }

    public static bool IsNullOrEmpty(this string subject)
    {
        return string.IsNullOrEmpty(subject);
    }

    public static string Repeat(this char subject, int count)
    {
        return new string(subject, count);
    }

    public static string Repeat(this string subject, int count)
    {
        return string.Concat(Enumerable.Repeat(subject, count));
    }

    public static string Splice(this string subject, int startIndex, string replacement)
    {
        return Splice(subject, startIndex, replacement, replacement.Length);
    }

    public static string Splice(this string subject, int startIndex, string replacement, int removeCount)
    {
        return subject.Remove(startIndex, removeCount).Insert(startIndex, replacement);
    }

    public static string StripAllIndent(this string subject)
    {
        return subject.Lines()
                      .Select(l => l.TrimStart())
                      .ToJoinNewlineString();
    }

    public static string StripIndent(this string subject)
    {
        IReadOnlyList<string> indentLines = _indentRegex.NonCachingMatches(subject)
                                                        .Select(m => m.Groups[1].Value)
                                                        .ToReadOnlyList();
        if (indentLines.None()) return subject;

        int minIndent = indentLines.Min(i => i.Length);

        return subject.Lines()
                      .Select(line => line.Length <= minIndent
                                          ? line
                                          : line.Substring(minIndent))
                      .ToJoinNewlineString();
    }

    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || str.Length < 2) return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}

public static partial class StringExtension
{
    public static string HangIndent(this string subject, string body, string suffix = null)
    {
        if (string.IsNullOrEmpty(subject)) return subject ?? string.Empty;

        return $"{subject}{body.IndentSkipFirstLine(subject.Length)}{suffix}";
    }

    public static IEnumerable<string> Indent(this IEnumerable<string> subject, int size = 4, bool skipFirstLine = false)
    {
        return subject == null
                   ? []
                   : subject.Select((s, index) => skipFirstLine && index == 0 ? s : s.Indent(size));
    }

    public static string Indent(this string subject, int size = 4, bool skipFirstLine = false)
    {
        if (string.IsNullOrEmpty(subject) || size == 0) return subject ?? string.Empty;

        string indent = new(' ', size);

        return subject.Lines()
                      .Select((line, index) => index == 0 && skipFirstLine ? line : $"{indent}{line}")
                      .ToJoinNewlineString();
    }

    public static IEnumerable<string> IndentSkipFirstLine(this IEnumerable<string> subject, int length = 4)
    {
        return subject.Indent(size: length, skipFirstLine: true);
    }

    public static string IndentSkipFirstLine(this string subject, int size = 4)
    {
        return subject.Indent(size, skipFirstLine: true);
    }

    public static string LeadingBlankLines(this string subject, int count)
    {
        if (subject == null) return string.Empty;

        return Environment.NewLine.Repeat(count + 1) + subject.TrimStart();
    }

    public static string TrailingBlankLines(this string subject, int count)
    {
        subject ??= string.Empty;

        return subject.TrimEnd() + Environment.NewLine.Repeat(count + 1);
    }
}

public static partial class StringExtension
{
    public static string PadLinesLeft(this string subject, int width)
    {
        // ReSharper disable once UseNullPropagation
        if (subject == null) return null;

        int maxLength = subject.Lines()
                               .Max(l => l.Length);

        return subject.Indent(width - maxLength);
    }

    public static string PadLinesRight(this string subject, int width)
    {
        return subject?.Lines()
                       .Select(l => l.PadRight(width))
                       .ToJoinNewlineString()
               ?? string.Empty;
    }

    public static string RenderNumbered(this IEnumerable<string> items, int startId = 1)
    {
        if (items == null) return string.Empty;

        items = items.ToReadOnlyList();
        if (!items.Any()) return "<none>";

        return TextTableBuilder.Create()
                               .AddIdentityColumn(leftPadding: 0, rightPadding: 0)
                               .AddStaticColumn(":")
                               .AddColumn()
                               .AddDataRows(items.Select(i => i.Yield().ToReadOnlyList()).ToReadOnlyList())
                               .Render();
    }

    public static IEnumerable<string> TrimEnd(this IEnumerable<string> items)
    {
        return items == null
                   ? []
                   : items.Select(i => i.TrimEnd());
    }
}

public static partial class StringExtension
{
    public static string Compress(this string content)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(content).Compress());
    }

    public static string Decompress(this string base64Content)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64Content).Decompress());
    }

    public static string Sha256(this string content)
    {
        return Encoding.UTF8.GetBytes(content).Sha256();
    }
}

public static partial class StringExtension
{
    public static IEnumerable<string> Lines(this string subject)
    {
        using StringReader reader = new(subject);

        while (reader.Peek() != -1)
        {
            yield return reader.ReadLine();
        }
    }

    public static string PadBottom(this string subject, int lineCount)
    {
        int actualLineCount = subject.Lines().Count();

        if (actualLineCount > lineCount) return subject;

        return subject + Environment.NewLine.Repeat(lineCount - actualLineCount);
    }

    public static string PadTop(this string subject, int lineCount)
    {
        int actualLineCount = subject.Lines().Count();

        if (actualLineCount > lineCount) return subject;

        return Environment.NewLine.Repeat(lineCount - actualLineCount) + subject;
    }

    public static string ToWrapString(this string text, int? maxLength, int overflowIndentSize = 0, bool showGlyph = false)
    {
        if (maxLength == null || string.IsNullOrEmpty(text)) return text ?? string.Empty;

        return text.Wrap(maxLength: maxLength.Value, overflowIndentSize: overflowIndentSize, showGlyph: showGlyph)
                   .ToJoinNewlineString();
    }

    public static string TruncateLines(this string subject, int lineCount)
    {
        string[] lines = subject.Lines().ToArray();

        if (lines.Length <= lineCount) return subject;

        return lines.Take(lineCount - 1)
                    .ToJoinNewlineString()
               + Environment.NewLine
               + "...";
    }

    public static IEnumerable<string> Wrap(this string text, int maxLength, int overflowIndentSize = 0, bool showGlyph = false)
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
            List<string> lines = text.Lines().ToList();
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
            string chunk = text.Substring(0, maxLength);

            // If next char is a space, we can use the whole chunk and remove the space for the next line
            if (char.IsWhiteSpace(text[maxLength]))
            {
                AddChunk(chunk);
                text = text.Substring(chunk.Length + 1); // Remove chunk plus space from original string
            }
            else
            {
                int splitIndex = chunk.LastIndexOf(' '); // Find last space in chunk.

                // If space exists in string,
                if (splitIndex != -1) chunk = chunk.Substring(0, splitIndex); // Remove chars after space.

                text = text.Substring(chunk.Length + (splitIndex == -1 ? 0 : 1)); // Remove chunk plus space (if found) from original string
                AddChunk(chunk);
            }
        }

        return chunks;

        void AddChunk(string localChuck, bool lastLine = false)
        {
            string localIndent = index > 0 ? indent : string.Empty;
            chunks.Add(showGlyph
                           ? $"{localIndent}{localChuck}{(!lastLine ? wrapGlyph : string.Empty)}"
                           : $"{localIndent}{localChuck}");
        }
    }
}