namespace Koalas.Extensions;

public static class DirectoryInfoExtensions
{
    /// <summary>
    /// Get all files from <see cref="directory"/>.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="searchPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEnumerable<FileInfo> Files(
        this DirectoryInfo directory,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return directory.Yield().Files(searchPattern, options);
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
    ///     Reads all lines of a file and returns them as an enumerable collection of containing each line as a string.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    /// <summary>
    /// Get all text lines from files in directories.
    /// </summary>
    /// <param name="directories"></param>
    /// <param name="searchPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(
        this IEnumerable<DirectoryInfo> directories,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        foreach (DirectoryInfo directory in directories)
        {
            foreach (FileInfo file in directory.EnumerateFiles(searchPattern, options))
            {
                using StreamReader reader = new(file.FullName);
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }
    }
}