namespace Koalas.Extensions;

public static class FileInfoExtensions
{
    private static readonly JsonSerializerOptions _defaultCompactOptions = new()
    {
        WriteIndented = false,
    };

    /// <summary>
    ///     Appends lines to a file, and then closes the file. If the specified file does
    ///     not exist, this method creates a file, writes the specified lines to the file,
    ///     and then closes the file.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="contentLines"></param>
    /// <returns></returns>
    public static FileInfo AppendAllLines(this FileInfo subject, IEnumerable<string> contentLines)
    {
        File.AppendAllLines(subject.FullName, contentLines);

        return subject;
    }

    /// <summary>
    ///     Opens a file, appends the specified string to the file, and then closes the file.
    ///     If the file does not exist, this method creates a file, writes the specified
    ///     string to the file, then closes the file.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static FileInfo AppendAllText(this FileInfo subject, string content)
    {
        File.AppendAllText(subject.FullName, content);

        return subject;
    }

    /// <summary>
    ///     Converts <paramref name="item" /> to JSON line, appends the json line to a file, and then closes the file. If the
    ///     specified file does not exist, this method creates a file, writes the specified lines to the file,
    ///     and then closes the file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileInfo"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static FileInfo AppendJsonLine<T>(this FileInfo fileInfo, T item)
    {
        return fileInfo.AppendJsonLines(item.Yield());
    }

    /// <summary>
    ///     Converts <paramref name="items" /> to JSON lines, appends all lines to a file, and then closes the file. If the
    ///     specified file does not exist, this method creates a file, writes the specified lines to the file,
    ///     and then closes the file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileInfo"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static FileInfo AppendJsonLines<T>(this FileInfo fileInfo, IEnumerable<T> items)
    {
        return fileInfo.AppendAllLines(
            items.Select(item => JsonSerializer.Serialize(item, _defaultCompactOptions))
        );
    }

    /// <summary>
    ///     Appends lines to a file, and then closes the file. If the specified file does
    ///     not exist, this method creates a file, writes the specified lines to the file,
    ///     and then closes the file.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    public static FileInfo AppendLine(this FileInfo subject, string line)
    {
        File.AppendAllLines(subject.FullName, line.Yield());

        return subject;
    }

    public static FileInfo Clone(this FileInfo file)
    {
        return new FileInfo(file.FullName);
    }

    /// <summary>
    ///     Created the directory for the file if it does not exist.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static FileInfo EnsureDirectory(this FileInfo subject)
    {
        if (subject.Directory?.Exists == false)
            subject.Directory?.Create();

        subject.Refresh();

        return subject;
    }

    /// <summary>
    ///     Creates the file and directory if it does not exist.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static FileInfo EnsureFile(this FileInfo subject)
    {
        subject.EnsureDirectory();

        if (!subject.Exists)
        {
            subject.WriteAllText(string.Empty);
        }

        subject.Refresh();

        return subject;
    }

    /// <summary>
    ///     Gets the file name without the extension.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    public static string FilenameWithoutExtension(this FileInfo fileInfo)
    {
        return Path.GetFileNameWithoutExtension(fileInfo.Name);
    }

    /// <summary>
    ///     Parses the JSON content of the file into an object of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="files"></param>
    /// <returns></returns>
    public static IEnumerable<T> ParseJson<T>(this IEnumerable<FileInfo> files)
    {
        return files.Select(file => file.ReadAllText().ParseJson<T>());
    }

    /// <summary>
    ///     Parses the JSON content of the file into an object of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <returns></returns>
    public static T ParseJson<T>(this FileInfo file)
    {
        return file.ReadAllText().ParseJson<T>();
    }

    /// <summary>
    ///     Opens a text file, reads all lines of the file, and then closes the file.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    public static string ReadAllText(this FileInfo fileInfo)
    {
        return File.ReadAllText(fileInfo.FullName);
    }

    /// <summary>
    ///     Reads the JSON content of the file and deserializes it into an object of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static T? ReadJson<T>(this FileInfo subject)
    {
        return JsonSerializer.Deserialize<T>(subject.ReadAllText());
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

    /// <summary>
    ///     Enumerates all text lines in <paramref name="items" />. Each item string is split into a set of lines and all lines
    ///     are returned.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(this IEnumerable<string> items)
    {
        return items.SelectMany(c => c.Lines());
    }

    /// <summary>
    ///     Refreshes the state of the object.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static FileInfo Refresh(this FileInfo subject)
    {
        subject.Refresh();

        return subject;
    }

    /// <summary>
    ///     Writes the specified object as a JSON line to the file. If the file does not exist, it is created.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileInfo"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static FileInfo WriteAllJsonLine<T>(this FileInfo fileInfo, T content)
    {
        return fileInfo.WriteAllText(JsonSerializer.Serialize(content, _defaultCompactOptions));
    }

    /// <summary>
    ///     Writes the specified lines to the file. If the file or directory does not exist, it is created.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <param name="lines"></param>
    /// <returns></returns>
    public static FileInfo WriteAllLines(this FileInfo fileInfo, IEnumerable<string> lines)
    {
        if (fileInfo.Directory?.Exists == false)
            fileInfo.Directory.Create();

        File.WriteAllLines(fileInfo.FullName, lines);

        return fileInfo;
    }

    /// <summary>
    ///     Creates a new file, writes the specified string to the file, and then closes
    ///     the file. If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static FileInfo WriteAllText(this FileInfo fileInfo, string text)
    {
        if (fileInfo.Directory?.Exists == false)
            fileInfo.Directory.Create();

        File.WriteAllText(fileInfo.FullName, text);

        return fileInfo;
    }

    /// <summary>
    ///     Writes the lines to a directory, creating multiple directory partitions and files if necessary.
    /// </summary>
    /// <param name="lines">Input lines to be written.</param>
    /// <param name="rootDirectoryPath">Root directory path.</param>
    /// <param name="filePrefix">File prefix.</param>
    /// <param name="extension"></param>
    /// <param name="maxDirectoryLines">Maximum number of text lines for each directory.</param>
    /// <param name="maxFileLines">Maximum number of text lines for each file.</param>
    /// <param name="maxParallel">Maximum number of parallel file writing tasks.</param>
    /// <returns></returns>
    public static IReadOnlyList<FileInfo> WriteDirectoryLines(
        this IEnumerable<string> lines,
        string rootDirectoryPath,
        string? filePrefix = null,
        string extension = "json",
        int maxDirectoryLines = 1_000,
        int maxFileLines = 1
    )
    {
        filePrefix ??= $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_";

        if (!filePrefix.EndsWith("_"))
            filePrefix = $"{filePrefix}_";

        List<FileInfo> files = [];
        int fileId = 1;
        int partId = 1;
        lines = lines.ToReadOnlyList();

        while (true)
        {
            List<string> directoryLines =
            [
                .. lines.Skip((partId - 1) * maxDirectoryLines).Take(maxDirectoryLines),
            ];
            if (directoryLines.Count == 0)
                break;

            foreach (IEnumerable<string> fileLines in directoryLines.Partition(maxFileLines))
            {
                string partName = $"part{partId:00000}";
                string dirPath =
                    lines.Count() < maxDirectoryLines
                        ? rootDirectoryPath
                        : Path.Combine(rootDirectoryPath, partName);
                string filePath = Path.Combine(
                    dirPath,
                    $"{filePrefix}{partName}_file{fileId:00000}.{extension}"
                );
                FileInfo file = new(filePath);
                files.Add(file);
                file.WriteAllText(fileLines.ToJoinNewlineString());
                fileId++;
            }

            partId++;
        }

        return [.. files];
    }
}
