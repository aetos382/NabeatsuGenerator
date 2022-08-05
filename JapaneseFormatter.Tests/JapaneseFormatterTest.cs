using Xunit;

namespace JapaneseFormatter.Tests;

public class JapaneseFormatterTest
{
    [Theory]
    [InlineData(0, "ぜろ")]
    [InlineData(10002, "いちまんに")]
    [InlineData(10000002, "いっせんまんに")]
    [InlineData(2147483647, "にじゅういちおくよんせんななひゃくよんじゅうはちまんさんぜんろっぴゃくよんじゅうなな")]
    public void とりあえず雑なテスト(
        int value,
        string expected)
    {
        var actual = string.Format(
            JapaneseNumberFormatter.Default,
            "{0:H}",
            value);

        Assert.Equal(expected, actual);
    }
}
