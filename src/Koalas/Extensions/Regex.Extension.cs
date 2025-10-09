namespace Koalas.Extensions;

public static class RegexExtensions
{
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
            Match? match = null;
            foreach (Regex regex in regexes)
            {
                match = regex.Match(s, pos);
                if (match.Success)
                    break;
            }

            if (match == null)
                yield break;

            if (match.Success)
            {
                pos = match.Index + (match.Length == 0 ? 1 : match.Length);
                yield return match;
            }

            done = !match.Success || pos > s.Length;
        } while (!done);
    }
}
