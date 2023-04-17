﻿namespace Koalas.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public static partial class ExtensionsFiles {
    public static IEnumerable<TTarget> ParseJson<TTarget>(this IEnumerable<string> items) {
        return items.Select(JsonConvert.DeserializeObject<TTarget>);
    }

    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> items, int size) {
        items = items as IReadOnlyCollection<T> ?? items.ToList();

        var i = 0;
        while (true) {
            IEnumerable<T> ret = items.Skip(size * i).Take(size).ToList();
            if (ret.FirstOrDefault() == null) {
                break;
            }

            yield return ret;

            i++;
        }
    }

    public static IEnumerable<string> SerializeJson<T>(this IEnumerable<T> items, Formatting format = Formatting.None) {
        return items.Select(item => JsonConvert.SerializeObject(item, format));
    }
}

public static partial class ExtensionsFiles {
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
        IEnumerable<FileInfo> files = directoryPaths.SelectMany(directoryPath => new DirectoryInfo(directoryPath).EnumerateFiles(searchPattern, options));

        foreach (FileInfo file in files) {
            using StreamReader reader = new(file.FullName);
            while (!reader.EndOfStream) {
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

    public static IEnumerable<string> ReadFileLines(this IEnumerable<FileInfo> contents) {
        return contents.SelectMany(f => File.ReadLines(f.FullName));
    }

    public static IEnumerable<string> ReadLines(this IEnumerable<string> items) {
        return items.SelectMany(c => c.Split(new[] {
                                                       Environment.NewLine,
                                                       "\n",
                                                       "\r"
                                                   },
                                             StringSplitOptions.RemoveEmptyEntries));
    }

    public static IReadOnlyList<FileInfo> WriteLineFiles(this IEnumerable<string> lines,
                                                         string directoryPath,
                                                         string prefix = null,
                                                         string extension = "json",
                                                         int maxDirectoryLines = 1_000_000,
                                                         int maxFileLines = 1,
                                                         int? maxParallel = null) {
        prefix ??= $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_";

        if (!prefix.EndsWith("_")) {
            prefix = $"{prefix}_";
        }

        List<FileInfo> files = new();
        var fileId = 1;
        var partId = 1;
        lines = lines as IReadOnlyCollection<string> ?? lines.ToList();

        while (true) {
            var directoryPartition = lines.Skip((partId - 1) * maxDirectoryLines).Take(maxDirectoryLines).ToList();
            if (!directoryPartition.Any()) {
                break;
            }

            foreach (IEnumerable<string> filePartition in directoryPartition.Partition(maxFileLines)) {
                var part = $"part{partId:00000}";
                string dirPath = lines.Count() < maxDirectoryLines ? directoryPath : Path.Combine(directoryPath, part);
                //var dirPath = Path.Combine(directoryPath, part);
                FileInfo file = new(Path.Combine(dirPath, $"{prefix}{part}_file{fileId:00000}.{extension}"));
                files.Add(file);

                if (file.Directory?.Exists == false) {
                    file.Directory.Create();
                }

                File.WriteAllText(file.FullName, string.Join(Environment.NewLine, filePartition.ToList()));
                fileId++;
            }

            partId++;
        }

        return files.ToList();
    }
}

public class FileMetaInfo {
    public string Content { get; set; }

    public FileInfo File { get; set; }
}