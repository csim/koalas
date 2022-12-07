namespace Koalas;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static partial class Extensions {
    public static IEnumerable<FileInfo> Files(IEnumerable<string> directoryPaths,
                                              string searchPattern = "",
                                              SearchOption options = SearchOption.TopDirectoryOnly) {
        return directoryPaths.SelectMany(directoryPath => new DirectoryInfo(directoryPath).EnumerateFiles(searchPattern, options));
    }

    public static IEnumerable<FileInfo> Files(string directoryPath,
                                              string searchPattern = "",
                                              SearchOption options = SearchOption.TopDirectoryOnly) {
        return Files(new[] { directoryPath }, searchPattern, options);
    }

    public static IReadOnlyList<T> ForAll<T>(this IEnumerable<T> items,
                                             Action<T> action) {
        var list = items.CoerceList();
        foreach (var item in list) {
            action(item);
        }

        return list;
    }

    public static IEnumerable<string> ReadDirectoryFiles(string directoryPath,
                                                         string searchPattern = "",
                                                         SearchOption options = SearchOption.TopDirectoryOnly) {
        return ReadDirectoryFiles(new[] { directoryPath }, searchPattern, options);
    }

    public static IEnumerable<string> ReadDirectoryFiles(IEnumerable<string> directoryPaths,
                                                         string searchPattern = "",
                                                         SearchOption options = SearchOption.TopDirectoryOnly) {
        return directoryPaths.SelectMany(directoryPath => new DirectoryInfo(directoryPath).EnumerateFiles(searchPattern, options)
                                                                                          .Select(file => File.ReadAllText(file.FullName)));
    }

    public static IEnumerable<string> ReadFileLines(string directoryPath,
                                                    string searchPattern = "",
                                                    SearchOption options = SearchOption.TopDirectoryOnly) {
        return ReadFileLines(new[] { directoryPath }, searchPattern, options);
    }

    public static IEnumerable<string> ReadFileLines(IEnumerable<string> directoryPaths,
                                                    string searchPattern = "",
                                                    SearchOption options = SearchOption.TopDirectoryOnly) {
        return directoryPaths.SelectMany(directoryPath => new DirectoryInfo(directoryPath).EnumerateFiles(searchPattern, options))
                             .SelectMany(file => File.ReadAllLines(file.FullName));
    }

    public static IEnumerable<string> ReadFileLines(this IEnumerable<FileInfo> contents) {
        return contents.SelectMany(f => File.ReadLines(f.FullName));
    }

    public static IEnumerable<string> ReadFiles(this IEnumerable<FileInfo> items) {
        return items.SelectParallel(item => File.ReadAllText(item.FullName));
    }

    public static IEnumerable<string> ReadLines(this IEnumerable<string> items) {
        return items.SelectMany(c => c.Split(new[] {
                                                       Environment.NewLine,
                                                       "\n",
                                                       "\r"
                                                   },
                                             StringSplitOptions.RemoveEmptyEntries));
    }

    public static IReadOnlyList<FileInfo> WriteFiles(this IEnumerable<string> items,
                                                     string directoryPath,
                                                     string prefix = null,
                                                     string extension = "json",
                                                     int maxDirectoryFiles = 1,
                                                     int maxFileObjects = 1,
                                                     int? maxParallel = null) {
        var list = items.CoerceList();

        if (prefix == null)
            prefix = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_";
        else if (!prefix.EndsWith("_")) prefix = $"{prefix}_";

        var files = new List<KoalasFileInfo>();
        var directoryPartId = 1;

        var directoryPartitions = list.Partition(maxDirectoryFiles).ToList();
        var directoryPartitionCount = directoryPartitions.Count;

        foreach (var directoryPartition in directoryPartitions) {
            var filePartId = 1;

            files.AddRange(from filePartition in directoryPartition.Partition(maxFileObjects)
                           let part = $"part{directoryPartId:00000}"
                           let filename = $"{prefix}{part}_file{filePartId++:00000}.{extension}"
                           let dirPath = directoryPartitionCount == 1 ? directoryPath : Path.Combine(directoryPath, part)
                           select new KoalasFileInfo {
                                                         Content = string.Join(Environment.NewLine, filePartition),
                                                         File = new FileInfo(Path.Combine(dirPath, filename))
                                                     });

            directoryPartId++;
        }

        files.ForAllParallel(file => {
                                 if (file.File.Directory?.Exists == false) file.File.Directory.Create();

                                 File.WriteAllText(file.File.FullName, file.Content);
                             },
                             maxParallel: maxParallel);

        return files.Select(f => f.File).ToList();
    }
}

public class KoalasFileInfo {
    public string Content { get; set; }

    public FileInfo File { get; set; }
}
