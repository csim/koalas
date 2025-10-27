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
    /// Transform <see cref="directoryPahts"/> into <see cref="DirectoryInfo"/>
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public static DirectoryInfo Directory(string directoryPath)
    {
        return new DirectoryInfo(directoryPath);
    }

    /// <summary>
    /// Find the given <see cref="searchFilename"/> in any ancestor directory.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="searchFileName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DirectoryInfo FindDirectory(Assembly assembly, string searchFilename)
    {
        DirectoryInfo assemblyDirectory = new(
            new FileInfo(assembly.Location).Directory?.FullName
                ?? throw new InvalidOperationException("Could not determine assembly location")
        );

        return assemblyDirectory
            .Ancestors()
            .FirstOrDefault(dir => File.Exists(Path.Combine(dir.FullName, searchFilename)));
    }
}