namespace Koalas;

public static class DirectoryHelper
{
    public static DirectoryInfo? SolutionDirectory(Assembly assembly, string solutionFileName)
    {
        DirectoryInfo assemblyDirectory = new(
            new FileInfo(assembly.Location).Directory?.FullName
                ?? throw new InvalidOperationException("Could not determine assembly location")
        );

        return assemblyDirectory
            .Ancestors()
            .FirstOrDefault(dir => File.Exists(Path.Combine(dir.FullName, solutionFileName)));
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
