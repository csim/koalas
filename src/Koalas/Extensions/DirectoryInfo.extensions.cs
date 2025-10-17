namespace Koalas.Extensions;

public static class DirectoryInfoExtensions
{
    /// <summary>
    /// Get all text lines from files in a directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="searchPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(
        this DirectoryInfo directory,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return directory.Yield().ReadLines(searchPattern, options);
    }

    /// <summary>
    /// Get all files from <see cref="directories"/>.
    /// </summary>
    /// <param name="directories"></param>
    /// <param name="searchPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEnumerable<FileInfo> Files(
        this IEnumerable<DirectoryInfo> directories,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        foreach (DirectoryInfo directory in directories)
        {
            foreach (FileInfo file in directory.EnumerateFiles(searchPattern, options))
            {
                yield return file;
            }
        }
    }
}
