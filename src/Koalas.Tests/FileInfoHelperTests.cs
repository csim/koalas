using Koalas.Extensions;

namespace Koalas.Tests;

public sealed class FileInfoHelperTests : IDisposable
{
    private readonly DirectoryInfo _tempDirectory;
    private readonly string _tempDirectoryPath;

    public FileInfoHelperTests()
    {
        _tempDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            $"FileInfoHelperTests_{Guid.NewGuid()}"
        );
        _tempDirectory = Directory.CreateDirectory(_tempDirectoryPath);
    }

    public void Dispose()
    {
        if (_tempDirectory.Exists)
        {
            _tempDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public void FileInfoHelper_IsStaticClass()
    {
        // Arrange & Act & Assert
        Type type = typeof(FileInfoHelper);

        Assert.True(type.IsClass);
        Assert.True(type.IsAbstract);
        Assert.True(type.IsSealed);
    }

    [Fact]
    public void Files_IsLazyEvaluated()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file1.txt"), "content1");

        // Act
        IEnumerable<FileInfo> result = FileInfoHelper.Files(_tempDirectory);

        // Create another file after getting the enumerable
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file2.txt"), "content2");

        // Assert - The enumerable should include the new file when evaluated
        List<FileInfo> list = [.. result];
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Files_WithEmptyDirectory_ReturnsEmptyCollection()
    {
        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(_tempDirectory)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Files_WithLargeNumberOfFiles_HandlesEfficiently()
    {
        // Arrange
        for (int i = 0; i < 100; i++)
        {
            File.WriteAllText(Path.Combine(_tempDirectoryPath, $"file{i}.txt"), $"content{i}");
        }

        // Act
        IEnumerable<FileInfo> result = FileInfoHelper.Files(_tempDirectory);

        // Assert - Test lazy evaluation
        Assert.NotNull(result);

        // Force evaluation
        List<FileInfo> list = [.. result];
        Assert.Equal(100, list.Count);
    }

    [Fact]
    public void Files_WithMultipleDirectories_ReturnsFilesFromAllDirectories()
    {
        // Arrange
        DirectoryInfo dir1 = Directory.CreateDirectory(Path.Combine(_tempDirectoryPath, "dir1"));
        DirectoryInfo dir2 = Directory.CreateDirectory(Path.Combine(_tempDirectoryPath, "dir2"));

        File.WriteAllText(Path.Combine(dir1.FullName, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(dir2.FullName, "file2.txt"), "content2");

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files([dir1, dir2])];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, static f => f.Name == "file1.txt");
        Assert.Contains(result, static f => f.Name == "file2.txt");
    }

    [Fact]
    public void Files_WithRecursiveOption_ReturnsFilesFromSubdirectories()
    {
        // Arrange
        DirectoryInfo subDir = _tempDirectory.CreateSubdirectory("subdir");
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(subDir.FullName, "file2.txt"), "content2");

        // Act
        List<FileInfo> result =
        [
            .. FileInfoHelper.Files(_tempDirectory, "", SearchOption.AllDirectories),
        ];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, static f => f.Name == "file1.txt");
        Assert.Contains(result, static f => f.Name == "file2.txt");
    }

    [Fact]
    public void Files_WithSearchPattern_FiltersCorrectly()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file2.cs"), "content2");
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file3.txt"), "content3");

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(_tempDirectory, "*.txt")];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, static f => Assert.Equal(".txt", f.Extension));
    }

    [Fact]
    public void Files_WithSingleDirectory_ReturnsFileInfoCollection()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file2.txt"), "content2");

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(_tempDirectory)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, static f => Assert.True(f.Exists));
        Assert.Contains(result, static f => f.Name == "file1.txt");
        Assert.Contains(result, static f => f.Name == "file2.txt");
    }

    [Fact]
    public void Files_WithSpecialCharactersInFilename_HandlesCorrectly()
    {
        // Arrange
        string specialFileName = "file with spaces & symbols!.txt";
        File.WriteAllText(Path.Combine(_tempDirectoryPath, specialFileName), "content");

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(_tempDirectory)];

        // Assert
        Assert.Single(result);
        Assert.Equal(specialFileName, result[0].Name);
    }

    [Fact]
    public void ReadLines_IsLazyEvaluated()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file1.txt"), "line1");

        // Act
        IEnumerable<string> result = _tempDirectory.ReadLines();

        // Create another file after getting the enumerable
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file2.txt"), "line2");

        // Assert - The enumerable should include lines from the new file
        List<string> list = [.. result];
        Assert.Equal(2, list.Count);
        Assert.Contains("line1", list);
        Assert.Contains("line2", list);
    }

    [Fact]
    public void ReadLines_MultipleFileInfos_ReturnsAllLines()
    {
        // Arrange
        string file1Path = Path.Combine(_tempDirectoryPath, "file1.txt");
        string file2Path = Path.Combine(_tempDirectoryPath, "file2.txt");

        File.WriteAllText(file1Path, "line1\nline2");
        File.WriteAllText(file2Path, "line3\nline4");

        FileInfo[] files = [new(file1Path), new(file2Path)];

        // Act
        List<string> result = [.. files.ReadLines()];

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains("line1", result);
        Assert.Contains("line2", result);
        Assert.Contains("line3", result);
        Assert.Contains("line4", result);
    }

    [Fact]
    public void ReadLines_SingleFileInfo_ReturnsFileLines()
    {
        // Arrange
        string filePath = Path.Combine(_tempDirectoryPath, "test.txt");
        File.WriteAllText(filePath, "line1\nline2\nline3");
        FileInfo fileInfo = new(filePath);

        // Act
        List<string> result = [.. fileInfo.ReadLines()];

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
    }

    [Fact]
    public void ReadLines_WithDifferentLineEndings_HandlesCorrectly()
    {
        // Arrange
        string filePath = Path.Combine(_tempDirectoryPath, "mixed.txt");
        string content = "line1\nline2\r\nline3\rline4";
        File.WriteAllText(filePath, content);

        FileInfo fileInfo = new(filePath);

        // Act
        List<string> result = [.. fileInfo.ReadLines()];

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
        Assert.Equal("line4", result[3]);
    }

    [Fact]
    public void ReadLines_WithEmptyFile_ReturnsEmptyCollection()
    {
        // Arrange
        string filePath = Path.Combine(_tempDirectoryPath, "empty.txt");
        File.WriteAllText(filePath, "");
        FileInfo fileInfo = new(filePath);

        // Act
        List<string> result = [.. fileInfo.ReadLines()];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ReadLines_WithMultipleDirectories_ReturnsLinesFromAllFiles()
    {
        // Arrange
        DirectoryInfo[] dirs =
        [
            Directory.CreateDirectory(Path.Combine(_tempDirectoryPath, "dir1")),
            Directory.CreateDirectory(Path.Combine(_tempDirectoryPath, "dir2")),
        ];

        File.WriteAllText(Path.Combine(dirs[0].FullName, "file1.txt"), "line1\nline2");
        File.WriteAllText(Path.Combine(dirs[1].FullName, "file2.txt"), "line3\nline4");

        // Act
        List<string> result = [.. dirs.ReadLines()];

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains("line1", result);
        Assert.Contains("line2", result);
        Assert.Contains("line3", result);
        Assert.Contains("line4", result);
    }

    [Fact]
    public void ReadLines_WithNonExistentFile_ThrowsException()
    {
        // Arrange
        FileInfo nonExistentFile = new(Path.Combine(_tempDirectoryPath, "nonexistent.txt"));

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => nonExistentFile.ReadLines().ToList());
    }

    [Fact]
    public void ReadLines_WithSearchPattern_FiltersFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file1.txt"), "line1\nline2");
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file2.cs"), "line3\nline4");

        // Act
        List<string> result = [.. _tempDirectory.ReadLines("*.txt")];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("line1", result);
        Assert.Contains("line2", result);
        Assert.DoesNotContain("line3", result);
        Assert.DoesNotContain("line4", result);
    }

    [Fact]
    public void ReadLines_WithSingleDirectory_ReturnsAllLines()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file1.txt"), "line1\nline2");
        File.WriteAllText(Path.Combine(_tempDirectoryPath, "file2.txt"), "line3\nline4");

        // Act
        List<string> result = [.. _tempDirectory.ReadLines()];

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains("line1", result);
        Assert.Contains("line2", result);
        Assert.Contains("line3", result);
        Assert.Contains("line4", result);
    }

    [Fact]
    public void ReadLines_WithUtf8Content_HandlesCorrectly()
    {
        // Arrange
        string filePath = Path.Combine(_tempDirectoryPath, "utf8.txt");
        string content = "Hello 世界\nBonjour 🌍";
        File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);

        FileInfo fileInfo = new(filePath);

        // Act
        List<string> result = [.. fileInfo.ReadLines()];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello 世界", result[0]);
        Assert.Equal("Bonjour 🌍", result[1]);
    }
}
