namespace Koalas;

public static class FileInfoHelper
{
    public static IEnumerable<FileInfo> Files(
        IEnumerable<string> directoryPaths,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return directoryPaths.SelectMany(directoryPath =>
            new DirectoryInfo(directoryPath).EnumerateFiles(searchPattern, options)
        );
    }

    public static IEnumerable<FileInfo> Files(
        string directoryPath,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return Files([directoryPath], searchPattern, options);
    }

    public static IEnumerable<string> ReadDirectoryFiles(
        string directoryPath,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return ReadDirectoryFiles([directoryPath], searchPattern, options);
    }

    public static IEnumerable<string> ReadDirectoryFiles(
        IEnumerable<string> directoryPaths,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return directoryPaths.SelectMany(directoryPath =>
            new DirectoryInfo(directoryPath)
                .EnumerateFiles(searchPattern, options)
                .Select(file => File.ReadAllText(file.FullName))
        );
    }

    public static IEnumerable<string> ReadFileLines(
        string directoryPath,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        return ReadFileLines([directoryPath], searchPattern, options);
    }

    public static IEnumerable<string> ReadFileLines(
        IEnumerable<string> directoryPaths,
        string searchPattern = "",
        SearchOption options = SearchOption.TopDirectoryOnly
    )
    {
        IEnumerable<FileInfo> files = directoryPaths.SelectMany(directoryPath =>
            new DirectoryInfo(directoryPath).EnumerateFiles(searchPattern, options)
        );

        foreach (FileInfo file in files)
        {
            using StreamReader reader = new(file.FullName);
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }

        //foreach (var file in files) {
        //    IEnumerable<string> lines = File.ReadLines(file.FullName);
        //    foreach (string line in lines) {
        //        yield return line;
        //    }
        //}
    }

    public static IEnumerable<string> ReadFileLines(this IEnumerable<FileInfo> contents)
    {
        return contents.SelectMany(static f => File.ReadLines(f.FullName));
    }

    public static IEnumerable<string> ReadLines(this IEnumerable<string> items)
    {
        return items.SelectMany(static c =>
            c.Split([Environment.NewLine, "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)
        );
    }

    public static IReadOnlyList<FileInfo> WriteFileLines(
        this IEnumerable<string> lines,
        string directoryPath,
        string? prefix = null,
        string extension = "json",
        int maxDirectoryLines = 1_000_000,
        int maxFileLines = 1
    )
    {
        prefix ??= $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_";

        if (!prefix.EndsWith('_'))
            prefix = $"{prefix}_";

        List<FileInfo> files = [];
        int fileId = 1;
        int partId = 1;
        lines = lines.ToReadOnlyList();

        while (true)
        {
            List<string> directoryPartition =
            [
                .. lines.Skip((partId - 1) * maxDirectoryLines).Take(maxDirectoryLines),
            ];
            if (directoryPartition.Count == 0)
                break;

            foreach (
                IEnumerable<string> filePartition in directoryPartition.Partition(maxFileLines)
            )
            {
                string part = $"part{partId:00000}";
                string dirPath =
                    lines.Count() < maxDirectoryLines
                        ? directoryPath
                        : Path.Combine(directoryPath, part);
                //var dirPath = Path.Combine(directoryPath, part);
                FileInfo file = new(
                    Path.Combine(dirPath, $"{prefix}{part}_file{fileId:00000}.{extension}")
                );
                files.Add(file);

                if (file.Directory?.Exists == false)
                    file.Directory.Create();

                File.WriteAllText(
                    file.FullName,
                    string.Join(Environment.NewLine, filePartition.ToList())
                );
                fileId++;
            }

            partId++;
        }

        return [.. files];
    }
}
