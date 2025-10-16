namespace Koalas;

public static class DirectoryHelper
{
    public static IEnumerable<DirectoryInfo> Directories(IEnumerable<string> directoryPaths)
    {
        foreach (string directoryPath in directoryPaths)
        {
            yield return new DirectoryInfo(directoryPath);
        }
    }

    public static DirectoryInfo Directory(string directoryPath)
    {
        return new DirectoryInfo(directoryPath);
    }

    public static DirectoryInfo FindDirectory(Assembly assembly, string searchFileName)
    {
        DirectoryInfo assemblyDirectory = new(
            new FileInfo(assembly.Location).Directory?.FullName
                ?? throw new InvalidOperationException("Could not determine assembly location")
        );

        return assemblyDirectory
            .Ancestors()
            .FirstOrDefault(dir => File.Exists(Path.Combine(dir.FullName, searchFileName)));
    }

    private static IEnumerable<DirectoryInfo> Ancestors(this DirectoryInfo directory)
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
}
