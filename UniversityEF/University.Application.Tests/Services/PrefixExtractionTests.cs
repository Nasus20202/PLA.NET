using Xunit;

namespace University.Application.Tests.Services;

public class PrefixExtractionTests
{
    [Theory]
    [InlineData("S1001", "S")]
    [InlineData("P101", "P")]
    [InlineData("D5", "D")]
    [InlineData("SF123", "SF")]
    [InlineData("PD456", "PD")]
    [InlineData("PH789", "PH")]
    [InlineData("ABC999", "ABC")]
    public void ExtractPrefix_ShouldReturnCorrectPrefix(
        string universityIndex,
        string expectedPrefix
    )
    {
        // Arrange & Act
        var prefix = ExtractPrefix(universityIndex);

        // Assert
        Assert.Equal(expectedPrefix, prefix);
    }

    private static string ExtractPrefix(string universityIndex)
    {
        // Extract non-digit prefix from index (e.g., "S1001" -> "S", "SF123" -> "SF")
        int i = 0;
        while (i < universityIndex.Length && !char.IsDigit(universityIndex[i]))
        {
            i++;
        }
        return universityIndex.Substring(0, i);
    }
}
