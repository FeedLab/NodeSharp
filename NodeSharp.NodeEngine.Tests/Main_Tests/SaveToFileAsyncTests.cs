using FluentAssertions;
using JetBrains.Annotations;

namespace NodeSharp.NodeEngine.Tests.Main_Tests;

[TestSubject(typeof(NodeEngine.Main))]
public class SaveToFileAsyncTests
{
    private const string TestFilePath = "test.json";
    private const string InvalidPath = "invalid/path/test.json";

    public SaveToFileAsyncTests()
    {
        CleanupTestFiles();
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldSaveDataToSpecifiedPath()
    {
        // Arrange
        var main = new NodeEngine.Main();

        // Act
        await main.SaveToFileAsync(TestFilePath);

        // Assert
        File.Exists(TestFilePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(TestFilePath);
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldThrowException_WhenPathIsInvalid()
    {
        // Arrange
        var main = new NodeEngine.Main();

        // Act
        var act = () => main.SaveToFileAsync(InvalidPath);

        // Assert
        await act.Should().ThrowAsync<IOException>();
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldCreateFile_WhenDataIsEmpty()
    {
        // Arrange
        var main = new NodeEngine.Main();
        main.Clear();

        // Act
        await main.SaveToFileAsync(TestFilePath);

        // Assert
        File.Exists(TestFilePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(TestFilePath);
        content.Should().Contain("\"Nodes\": []");

    }

    private void CleanupTestFiles()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }
}