namespace Koalas;

public static class DirectoryInfoHelper
{
    /// <summary>
    /// Enumerate all parent directories
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<DirectoryInfo> Ancestors(this DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            throw new ArgumentException($"{directory} not found.");
        }

        DirectoryInfo? currentDirectory = directory.Parent;

        while (currentDirectory != null)
        {
            yield return currentDirectory;

            currentDirectory = currentDirectory.Parent;
        }
    }

    /// <summary>
    /// Transform <see cref="directoryPahts"/> into <see cref="DirectoryInfo"/>
    /// </summary>
    /// <param name="directoryPaths"></param>
    /// <returns></returns>
    public static IEnumerable<DirectoryInfo> Directories(IEnumerable<string> directoryPaths)
    {
        foreach (string directoryPath in directoryPaths)
        {
            yield return new DirectoryInfo(directoryPath);
        }
    }

    /// <summary>
    /// Get directory from <see cref="path"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DirectoryInfo Directory(string path)
    {
        return new DirectoryInfo(path);
    }

    /// <summary>
    /// Find the directory where <see cref="filename"/> exists in any ancestor directory.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="ancestoryDirectoryName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DirectoryInfo FindAncestorDirectory(
        DirectoryInfo directory,
        string ancestoryDirectoryName
    )
    {
        return directory
            .Ancestors()
            .FirstOrDefault(dir =>
                dir.Name.Equals(ancestoryDirectoryName, StringComparison.OrdinalIgnoreCase)
            );
    }

    /// <summary>
    /// Find the directory where <see cref="filename"/> exists in any ancestor directory.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DirectoryInfo FindFileAncestorDirectory(DirectoryInfo directory, string filename)
    {
        return directory
            .Ancestors()
            .FirstOrDefault(dir => File.Exists(Path.Combine(dir.FullName, filename)));
    }
}
