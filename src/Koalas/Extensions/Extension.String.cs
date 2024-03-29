﻿namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static partial class Extension {
    public static IEnumerable<string> AllPrefixes(this string subject, int? endIndex = null) {
        return from i in Enumerable.Range(1, endIndex == null ? subject.Length : Math.Min(endIndex.Value, subject.Length))
               select subject.Substring(0, i);
    }

    public static IEnumerable<string> AllSubstrings(this string subject) {
        return from i in Enumerable.Range(0, subject.Length)
               from j in Enumerable.Range(1, subject.Length - i)
               select subject.Substring(i, j);
    }

    public static IEnumerable<string> AllSuffixes(this string subject, int startIndex = 1) {
        return from i in Enumerable.Range(startIndex, subject.Length - startIndex)
               select subject.Substring(i);
    }

    public static IEnumerable<T> AppendDistinct<T>(this IEnumerable<T> source, T target) {
        return source.Union(target.Yield());
    }

    public static string Concat(this string subject, string value) {
        return string.Concat(subject, value);
    }

    public static string Concat(this string subject, IEnumerable<string> values) {
        return string.Concat(subject, string.Concat(values));
    }

    public static bool ContainsIgnoreCase(this string subject, string target) {
        return subject.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static int IndexOfIgnoreCase(this string subject, string target) {
        return subject.IndexOf(target, StringComparison.OrdinalIgnoreCase);
    }

    public static int IndexOfOrdinal(this string subject, string target) {
        return subject.IndexOf(target, StringComparison.Ordinal);
    }

    public static string Repeat(this char subject, int count) {
        return new string(subject, count);
    }

    public static string Repeat(this string subject, int count) {
        return string.Concat(Enumerable.Repeat(subject, count));
    }

    public static string Splice(this string subject, int startIndex, string replacement) {
        return Splice(subject, startIndex, replacement, replacement.Length);
    }

    public static string Splice(this string subject, int startIndex, string replacement, int removeCount) {
        return subject.Remove(startIndex, removeCount).Insert(startIndex, replacement);
    }

    public static Regex ToRegex(this string pattern, bool ignoreCase) {
        RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline;
        if (ignoreCase) {
            options |= RegexOptions.IgnoreCase;
        }

        return ToRegex(pattern, options);
    }

    public static Regex ToRegex(this string pattern,
                                RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline) {
        return new Regex(pattern, options);
    }
}

public static partial class Extension {
    public static string Indent(this string subject, int length = 4, bool skipFirstLine = false) {
        string indent = ' '.Repeat(length);
        string replacement = subject?.Replace(_newline, $"{_newline}{indent}");

        return skipFirstLine
                   ? replacement
                   : $"{indent}{replacement}";
    }

    public static string IndentSkipFirstLine(this string subject, int length = 4) {
        return subject.Indent(length, skipFirstLine: true);
    }
}