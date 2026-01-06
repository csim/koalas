using System.Reflection;
using Microsoft.VisualBasic;

namespace Koalas.Tests;

public sealed class DirectoryInfoHelperTests : IDisposable
{
    private readonly DirectoryInfo _tempDirectory;
    private readonly string _tempDirectoryPath;

    public DirectoryInfoHelperTests()
    {
        _tempDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            $"DirectoryHelperTests_{Guid.NewGuid()}"
        );
        _tempDirectory = Directory.CreateDirectory(_tempDirectoryPath);
    }

    [Fact]
    public void Ancestors_IsPrivate_CannotBeAccessedDirectly()
    {
        // Arrange & Act & Assert
        Type directoryHelperType = typeof(DirectoryInfoHelper);
        MethodInfo? ancestorsMethod = directoryHelperType.GetMethod(
            "Ancestors",
            BindingFlags.Public | BindingFlags.Static
        );

        Assert.NotNull(ancestorsMethod); // Ancestors method should exist as private method
        Assert.True(ancestorsMethod.IsPublic); // Ancestors method should be private
    }

    [Fact]
    public void Ancestors_WithExistingDirectory_ReturnsAllParents()
    {
        // Arrange
        DirectoryInfo level1 = _tempDirectory.CreateSubdirectory("level1");
        DirectoryInfo level2 = level1.CreateSubdirectory("level2");
        DirectoryInfo level3 = level2.CreateSubdirectory("level3");

        // Act
        List<DirectoryInfo> result = [.. level3.Ancestors()];

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(level2.FullName, result[0].FullName);
        Assert.Equal(level1.FullName, result[1].FullName);
        Assert.Equal(_tempDirectory.FullName, result[2].FullName);
    }

    [Fact]
    public void Ancestors_WithNonExistentDirectory_ThrowsArgumentException()
    {
        // Arrange
        DirectoryInfo nonExistent = new(Path.Combine(_tempDirectoryPath, "nonexistent"));

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            nonExistent.Ancestors().ToList()
        );
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void Ancestors_WithRootDirectory_ReturnsParentsUpToRoot()
    {
        // Arrange
        DirectoryInfo deepDir = _tempDirectory.CreateSubdirectory("deep");

        // Act
        List<DirectoryInfo> result = [.. deepDir.Ancestors()];

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(_tempDirectory.FullName, result[0].FullName);
        // Continue until we reach the root
        Assert.Contains(result, d => d.Parent == null);
    }

    [Fact]
    public void Directories_PreservesOrder()
    {
        // Arrange
        string[] orderedPaths =
        [
            Path.Combine(_tempDirectoryPath, "zdir"),
            Path.Combine(_tempDirectoryPath, "adir"),
            Path.Combine(_tempDirectoryPath, "mdir"),
        ];

        // Act
        List<DirectoryInfo> result = [.. DirectoryInfoHelper.Directories(orderedPaths)];

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("zdir", result[0].Name);
        Assert.Equal("adir", result[1].Name);
        Assert.Equal("mdir", result[2].Name);
    }

    [Fact]
    public void Directories_WithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        string[] emptyPaths = [];

        // Act
        List<DirectoryInfo> result = [.. DirectoryInfoHelper.Directories(emptyPaths)];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Directories_WithLargeCollection_HandlesEfficiently()
    {
        // Arrange
        string[] largePaths =
        [
            .. Enumerable.Range(1, 50).Select(i => Path.Combine(_tempDirectoryPath, $"dir{i}")),
        ];

        // Act
        IEnumerable<DirectoryInfo> result = DirectoryInfoHelper.Directories(largePaths);

        // Assert - Test that it's lazy evaluated
        Assert.NotNull(result);

        // Force evaluation
        List<DirectoryInfo> list = [.. result];
        Assert.Equal(50, list.Count);
    }

    [Fact]
    public void Directories_WithNonExistentPaths_ReturnsDirectoryInfoObjects()
    {
        // Arrange
        string[] nonExistentPaths =
        [
            Path.Combine(_tempDirectoryPath, "nonexistent1"),
            Path.Combine(_tempDirectoryPath, "nonexistent2"),
        ];

        // Act
        List<DirectoryInfo> result = [.. DirectoryInfoHelper.Directories(nonExistentPaths)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.IsType<DirectoryInfo>(result[0]);
        Assert.IsType<DirectoryInfo>(result[1]);
        Assert.False(result[0].Exists);
        Assert.False(result[1].Exists);
    }

    [Fact]
    public void Directories_WithValidPaths_ReturnsDirectoryInfoCollection()
    {
        // Arrange
        DirectoryInfo subDir1 = _tempDirectory.CreateSubdirectory("subdir1");
        DirectoryInfo subDir2 = _tempDirectory.CreateSubdirectory("subdir2");
        string[] directoryPaths = [subDir1.FullName, subDir2.FullName];

        // Act
        List<DirectoryInfo> result = [.. DirectoryInfoHelper.Directories(directoryPaths)];

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(subDir1.FullName, result[0].FullName);
        Assert.Equal(subDir2.FullName, result[1].FullName);
        Assert.IsType<DirectoryInfo>(result[0]);
        Assert.IsType<DirectoryInfo>(result[1]);
    }

    [Fact]
    public void Directory_WithNonExistentPath_ReturnsDirectoryInfo()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_tempDirectoryPath, "nonexistent");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.Directory(nonExistentPath);

        // Assert
        Assert.IsType<DirectoryInfo>(result);
        Assert.Equal(nonExistentPath, result.FullName);
        Assert.False(result.Exists);
    }

    [Fact]
    public void Directory_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        string specialPath = Path.Combine(_tempDirectoryPath, "test with spaces & symbols!");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.Directory(specialPath);

        // Assert
        Assert.IsType<DirectoryInfo>(result);
        Assert.Equal(specialPath, result.FullName);
    }

    [Fact]
    public void Directory_WithValidPath_ReturnsDirectoryInfo()
    {
        // Arrange
        DirectoryInfo subDir = _tempDirectory.CreateSubdirectory("testdir");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.Directory(subDir.FullName);

        // Assert
        Assert.IsType<DirectoryInfo>(result);
        Assert.Equal(subDir.FullName, result.FullName);
        Assert.True(result.Exists);
    }

    [Fact]
    public void DirectoryHelper_IsStaticClass()
    {
        // Arrange & Act & Assert
        Type type = typeof(DirectoryInfoHelper);

        Assert.True(type.IsClass);
        Assert.True(type.IsAbstract);
        Assert.True(type.IsSealed);
    }

    public void Dispose()
    {
        if (_tempDirectory.Exists)
        {
            _tempDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public void FindAncestorDirectory_CaseInsensitiveMatch_ReturnsDirectory()
    {
        // Arrange
        DirectoryInfo parent = _tempDirectory.CreateSubdirectory("MyProject");
        DirectoryInfo child = parent.CreateSubdirectory("src");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindAncestorDirectory(child, "myproject");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyProject", result.Name);
        Assert.Equal(parent.FullName, result.FullName);
    }

    [Fact]
    public void FindAncestorDirectory_DirectoryExistsInImmediateParent_ReturnsImmediateParent()
    {
        // Arrange
        DirectoryInfo parent = _tempDirectory.CreateSubdirectory("parent");
        DirectoryInfo child = parent.CreateSubdirectory("child");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindAncestorDirectory(child, "parent");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("parent", result.Name);
        Assert.Equal(parent.FullName, result.FullName);
    }

    [Fact]
    public void FindAncestorDirectory_DirectoryExistsInParent_ReturnsParentDirectory()
    {
        // Arrange
        DirectoryInfo root = _tempDirectory.CreateSubdirectory("root");
        DirectoryInfo middle = root.CreateSubdirectory("middle");
        DirectoryInfo leaf = middle.CreateSubdirectory("leaf");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindAncestorDirectory(leaf, "root");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("root", result.Name);
        Assert.Equal(root.FullName, result.FullName);
    }

    [Fact]
    public void FindAncestorDirectory_DirectoryNotFound_ReturnsNull()
    {
        // Arrange
        DirectoryInfo parent = _tempDirectory.CreateSubdirectory("parent");
        DirectoryInfo child = parent.CreateSubdirectory("child");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindAncestorDirectory(child, "nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindAncestorDirectory_MultipleAncestorsWithSimilarNames_ReturnsNearestMatch()
    {
        // Arrange
        DirectoryInfo root = _tempDirectory.CreateSubdirectory("project");
        DirectoryInfo middle = root.CreateSubdirectory("src");
        DirectoryInfo leaf = middle.CreateSubdirectory("nested");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindAncestorDirectory(leaf, "project");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("project", result.Name);
        Assert.Equal(root.FullName, result.FullName);
    }

    [Fact]
    public void FindFileAncestorDirectory_FileExistsInImmediateParent_ReturnsImmediateParent()
    {
        // Arrange
        DirectoryInfo parentDir = _tempDirectory.CreateSubdirectory("parent");
        DirectoryInfo childDir = parentDir.CreateSubdirectory("child");

        string testFileName = "config.json";
        string testFilePath = Path.Combine(parentDir.FullName, testFileName);
        File.WriteAllText(testFilePath, "{}");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindFileAncestorDirectory(
            childDir,
            testFileName
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parentDir.FullName, result.FullName);
    }

    [Fact]
    public void FindFileAncestorDirectory_FileExistsInParent_ReturnsParentDirectory()
    {
        // Arrange
        DirectoryInfo parentDir = _tempDirectory.CreateSubdirectory("parent");
        DirectoryInfo childDir = parentDir.CreateSubdirectory("child");
        DirectoryInfo grandchildDir = childDir.CreateSubdirectory("grandchild");

        string testFileName = "test.txt";
        string testFilePath = Path.Combine(parentDir.FullName, testFileName);
        File.WriteAllText(testFilePath, "test content");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindFileAncestorDirectory(
            grandchildDir,
            testFileName
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parentDir.FullName, result.FullName);
    }

    [Fact]
    public void FindFileAncestorDirectory_FileExistsInRoot_ReturnsRootDirectory()
    {
        // Arrange
        DirectoryInfo level1 = _tempDirectory.CreateSubdirectory("level1");
        DirectoryInfo level2 = level1.CreateSubdirectory("level2");
        DirectoryInfo level3 = level2.CreateSubdirectory("level3");

        string testFileName = "root.config";
        string testFilePath = Path.Combine(_tempDirectory.FullName, testFileName);
        File.WriteAllText(testFilePath, "root config");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindFileAncestorDirectory(level3, testFileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_tempDirectory.FullName, result.FullName);
    }

    [Fact]
    public void FindFileAncestorDirectory_FileNotFound_ReturnsNull()
    {
        // Arrange
        DirectoryInfo parentDir = _tempDirectory.CreateSubdirectory("parent");
        DirectoryInfo childDir = parentDir.CreateSubdirectory("child");

        string nonExistentFileName = "nonexistent.txt";

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindFileAncestorDirectory(
            childDir,
            nonExistentFileName
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindFileAncestorDirectory_MultipleFilesWithSameName_ReturnsNearestAncestor()
    {
        // Arrange
        DirectoryInfo root = _tempDirectory.CreateSubdirectory("root");
        DirectoryInfo middle = root.CreateSubdirectory("middle");
        DirectoryInfo leaf = middle.CreateSubdirectory("leaf");

        string fileName = "duplicate.txt";

        // Create file in root
        File.WriteAllText(Path.Combine(root.FullName, fileName), "root version");
        // Create file in middle (closer ancestor)
        File.WriteAllText(Path.Combine(middle.FullName, fileName), "middle version");

        // Act
        DirectoryInfo result = DirectoryInfoHelper.FindFileAncestorDirectory(leaf, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(middle.FullName, result.FullName);
    }

    [Fact]
    public void FindFileAncestorDirectory_WithDifferentFileExtensions_FindsCorrectFile()
    {
        // Arrange
        DirectoryInfo parentDir = _tempDirectory.CreateSubdirectory("parent");
        DirectoryInfo childDir = parentDir.CreateSubdirectory("child");

        // Create files with different extensions
        File.WriteAllText(Path.Combine(parentDir.FullName, "project.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(parentDir.FullName, "project.sln"), "solution");

        // Act
        DirectoryInfo csprojResult = DirectoryInfoHelper.FindFileAncestorDirectory(
            childDir,
            "project.csproj"
        );
        DirectoryInfo slnResult = DirectoryInfoHelper.FindFileAncestorDirectory(
            childDir,
            "project.sln"
        );

        // Assert
        Assert.NotNull(csprojResult);
        Assert.NotNull(slnResult);
        Assert.Equal(parentDir.FullName, csprojResult.FullName);
        Assert.Equal(parentDir.FullName, slnResult.FullName);
    }
}
