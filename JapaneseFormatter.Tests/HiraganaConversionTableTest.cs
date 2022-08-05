using Xunit;

namespace JapaneseFormatter.Tests;

public class HiraganaConversionTableTest
{
    [Fact]
    public void ひらがなで_0は_ぜろ()
    {
        var actual = HiraganaConversionTable.Default.SpecialFormat(0);
        Assert.Equal("ぜろ", actual);
    }

    [Theory]
    [InlineData(0, 0, 0, "")]
    [InlineData(1, 0, 0, "")]
    [InlineData(1, 3, 0, "いっ")]
    [InlineData(8, 3, 0, "はっ")]
    [InlineData(1, 0, 1, "いち")]
    public void GetDigitテスト(
        int digit,
        int juUnit,
        int manUnit,
        string expected)
    {
        var actual = HiraganaConversionTable.Default.GetDigit(digit, juUnit, manUnit);
        Assert.Equal(expected, actual);
    }
}
