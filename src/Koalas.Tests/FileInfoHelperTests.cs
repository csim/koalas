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
    public void File_WithNonExistentPath_ReturnsFileInfo()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_tempDirectoryPath, "nonexistent.txt");

        // Act
        FileInfo result = FileInfoHelper.File(nonExistentPath);

        // Assert
        Assert.IsType<FileInfo>(result);
        Assert.Equal(nonExistentPath, result.FullName);
        Assert.False(result.Exists);
    }

    [Fact]
    public void File_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        string specialPath = Path.Combine(_tempDirectoryPath, "test file & symbols!.txt");

        // Act
        FileInfo result = FileInfoHelper.File(specialPath);

        // Assert
        Assert.IsType<FileInfo>(result);
        Assert.Equal(specialPath, result.FullName);
    }

    [Fact]
    public void File_WithValidPath_ReturnsFileInfo()
    {
        // Arrange
        string filePath = Path.Combine(_tempDirectoryPath, "test.txt");
        File.WriteAllText(filePath, "content");

        // Act
        FileInfo result = FileInfoHelper.File(filePath);

        // Assert
        Assert.IsType<FileInfo>(result);
        Assert.Equal(filePath, result.FullName);
        Assert.True(result.Exists);
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
    public void Files_FromFilePaths_IsLazyEvaluated()
    {
        // Arrange
        string file1Path = Path.Combine(_tempDirectoryPath, "file1.txt");
        File.WriteAllText(file1Path, "content");

        string[] filePaths = [file1Path];

        // Act
        IEnumerable<FileInfo> result = FileInfoHelper.Files(filePaths);

        // Create another file and add to array (demonstrates lazy evaluation)
        string file2Path = Path.Combine(_tempDirectoryPath, "file2.txt");
        File.WriteAllText(file2Path, "content2");

        // Assert - Only evaluates when enumerated
        Assert.NotNull(result);
        List<FileInfo> list = [.. result];
        Assert.Single(list); // Only includes the original file
    }

    [Fact]
    public void Files_FromFilePaths_PreservesOrder()
    {
        // Arrange
        string[] orderedPaths =
        [
            Path.Combine(_tempDirectoryPath, "zfile.txt"),
            Path.Combine(_tempDirectoryPath, "afile.txt"),
            Path.Combine(_tempDirectoryPath, "mfile.txt"),
        ];

        File.WriteAllText(orderedPaths[0], "z");
        File.WriteAllText(orderedPaths[1], "a");
        File.WriteAllText(orderedPaths[2], "m");

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(orderedPaths)];

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("zfile.txt", result[0].Name);
        Assert.Equal("afile.txt", result[1].Name);
        Assert.Equal("mfile.txt", result[2].Name);
    }

    [Fact]
    public void Files_FromFilePaths_WithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        string[] emptyPaths = [];

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(emptyPaths)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Files_FromFilePaths_WithNonExistentPaths_ReturnsFileInfoObjects()
    {
        // Arrange
        string[] nonExistentPaths =
        [
            Path.Combine(_tempDirectoryPath, "nonexistent1.txt"),
            Path.Combine(_tempDirectoryPath, "nonexistent2.txt"),
        ];

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(nonExistentPaths)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.IsType<FileInfo>(result[0]);
        Assert.IsType<FileInfo>(result[1]);
        Assert.False(result[0].Exists);
        Assert.False(result[1].Exists);
    }

    [Fact]
    public void Files_FromFilePaths_WithValidPaths_ReturnsFileInfoCollection()
    {
        // Arrange
        string file1Path = Path.Combine(_tempDirectoryPath, "file1.txt");
        string file2Path = Path.Combine(_tempDirectoryPath, "file2.txt");
        File.WriteAllText(file1Path, "content1");
        File.WriteAllText(file2Path, "content2");

        string[] filePaths = [file1Path, file2Path];

        // Act
        List<FileInfo> result = [.. FileInfoHelper.Files(filePaths)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(file1Path, result[0].FullName);
        Assert.Equal(file2Path, result[1].FullName);
        Assert.True(result[0].Exists);
        Assert.True(result[1].Exists);
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
        Assert.Contains(result, f => f.Name == "file1.txt");
        Assert.Contains(result, f => f.Name == "file2.txt");
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
        Assert.Contains(result, f => f.Name == "file1.txt");
        Assert.Contains(result, f => f.Name == "file2.txt");
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
        Assert.All(result, f => Assert.Equal(".txt", f.Extension));
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
        Assert.All(result, f => Assert.True(f.Exists));
        Assert.Contains(result, f => f.Name == "file1.txt");
        Assert.Contains(result, f => f.Name == "file2.txt");
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
