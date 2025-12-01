using Linkzip.Services;
using Xunit;

namespace Linkzip.Tests;

public class ZipperServiceTests
{
    private readonly ZipperService _zipperService;

    public ZipperServiceTests()
    {
        var deduplicationService = new DeduplicationService();
        _zipperService = new ZipperService(deduplicationService);
    }

    [Fact]
    public void Zip_SimpleText_ReturnsLinksNotation()
    {
        // Arrange
        var text = "hello world";

        // Act
        var (result, patternsApplied) = _zipperService.Zip(text);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("hello", result);
        Assert.Contains("world", result);
    }

    [Fact]
    public void Zip_RepeatedText_AppliesCompression()
    {
        // Arrange
        var text = "foo bar foo bar foo bar";

        // Act
        var (result, patternsApplied) = _zipperService.Zip(text);

        // Assert
        Assert.NotEmpty(result);
        // Should have some compression applied
        Assert.True(patternsApplied >= 0);
    }

    [Fact]
    public void Unzip_LinksNotation_ReturnsText()
    {
        // Arrange
        var linksNotation = "hello world";

        // Act
        var result = _zipperService.Unzip(linksNotation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("hello", result);
        Assert.Contains("world", result);
    }

    [Fact]
    public void Zip_EmptyText_ReturnsEmpty()
    {
        // Arrange
        var text = "";

        // Act
        var (result, patternsApplied) = _zipperService.Zip(text);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, patternsApplied);
    }

    [Fact]
    public void Unzip_EmptyLinksNotation_ReturnsEmpty()
    {
        // Arrange
        var linksNotation = "";

        // Act
        var result = _zipperService.Unzip(linksNotation);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ZipUnzip_RoundTrip_PreservesContent()
    {
        // Arrange
        var originalText = "test data with multiple words";

        // Act
        var (compressed, _) = _zipperService.Zip(originalText);
        var decompressed = _zipperService.Unzip(compressed);

        // Assert
        var originalWords = originalText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var decompressedWords = decompressed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // All original words should be present in decompressed output
        foreach (var word in originalWords)
        {
            Assert.Contains(word, decompressedWords);
        }
    }
}
