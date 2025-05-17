namespace Koalas.Extensions;

public static class RegexUtils
{
    /// <summary>
    ///     Returns the list of all <see cref="Match" />(es) of <paramref name="regex" /> in <paramref name="s" />.
    ///     Unlike Regex.Matches(Content) that creates many cache objects, this method does all the bookkeeping to
    ///     avoid creating these cache objects.
    /// </summary>
    /// <param name="regex">The matching <see cref="Regex" />.</param>
    /// <param name="s">The string to match against.</param>
    /// <returns>The list of all matches.</returns>
    public static IEnumerable<Match> NonCachingMatches(this Regex regex, string s)
    {
        int pos = 0;
        bool done;
        do
        {
            Match match = regex.Match(s, pos);
            if (match.Success)
            {
                pos = match.Index + (match.Length == 0 ? 1 : match.Length);
                yield return match;
            }

            done = !match.Success || pos > s.Length;
        } while (!done);
    }

    public static IEnumerable<Match> NonCachingMatches(this IReadOnlyList<Regex> regexes, string s)
    {
        int pos = 0;
        bool done;
        do
        {
            // avoiding LINQ here. Might be a premature optimization. This is equivalent to
            //Match match = regexes.Select(regex => regex.Match(s, pos)).FirstOrDefault();
            // and changing the checks for match.Success to match != null.
            Match match = null;
            foreach (Regex regex in regexes)
            {
                match = regex.Match(s, pos);
                if (match.Success) break;
            }

            if (match.Success)
            {
                pos = match.Index + (match.Length == 0 ? 1 : match.Length);
                yield return match;
            }

            done = !match.Success || pos > s.Length;
        } while (!done);
    }
}