namespace Outback;

public static class FileInfoHelper
{
    /// <summary>
    /// Get file from <see cref="path"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FileInfo File(string path)
    {
        return new FileInfo(path);
    }

    /// <summary>
    /// Get all files from a directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="searchPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEnumerable<FileInfo> Files(IEnumerable<string> filePaths)
    {
        foreach (string filePath in filePaths)
        {
            yield return File(filePath);
        }
    }

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
