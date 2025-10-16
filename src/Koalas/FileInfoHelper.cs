namespace Koalas;

public static class FileInfoHelper
{
    /// <summary>
    /// Get all files from a directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="searchPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEnumerable<FileInfo> Files(
        DirectoryInfo directory,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return Files([directory], searchPattern, options);
    }

    /// <summary>
    /// Get all files from directories.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="searchPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEnumerable<FileInfo> Files(
        IEnumerable<DirectoryInfo> directories,
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
        DirectoryInfo directory,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return ReadLines([directory], searchPattern, options);
    }

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

    /// <summary>
    /// Read lines from a text file.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(this FileInfo file)
    {
        return ReadLines([file]);
    }

    /// <summary>
    /// Read lines from a text file.
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(this IEnumerable<FileInfo> files)
    {
        foreach (FileInfo file in files)
        {
            using StreamReader reader = new(file.FullName);
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }
    }
}
