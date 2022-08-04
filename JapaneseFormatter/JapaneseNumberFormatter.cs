using System.Numerics;
using System.Text;

namespace JapaneseFormatter;

public class JapaneseNumberFormatter :
    IFormatProvider,
    ICustomFormatter
{
    public object? GetFormat(
        Type formatType)
    {
        if (formatType == typeof(ICustomFormatter))
        {
            return this;
        }

        return null;
    }

    public string Format(
        string format,
        object? arg,
        IFormatProvider? formatProvider)
    {
        if (arg is null)
        {
            return string.Empty;
        }

        if (format != Formats.Hiragana &&
            format != Formats.Katakana &&
            format != Formats.Kanji)
        {
            throw new FormatException();
        }

        if (arg is byte ||
            arg is sbyte ||
            arg is short ||
            arg is ushort ||
            arg is int ||
            arg is uint ||
            arg is long ||
            arg is ulong ||
            arg is BigInteger)
        {
        }

        if (arg is IFormattable formattable)
        {
            return formattable.ToString(format, formatProvider);
        }

        return arg.ToString();
    }

    static string ToJapaneseString(
        int value)
    {
        if (value == 0)
        {
            return "ぜろ";
        }

        var builder = new StringBuilder();

        var digits = new Stack<int>();

        while (value > 0)
        {
            digits.Push(value % 10);
            value /= 10;
        }

        var length = digits.Count;

        var manUnit = length switch {
            10 or 9 => 2,
            8 or 7 or 6 or 5 => 1,
            4 or 3 or 2 or 1 => 0
        };

        var juUnit = length switch {
            8 or 4 => 3,
            7 or 3 => 2,
            10 or 6 or 2 => 1,
            9 or 5 or 1 => 0
        };

        while (digits.Any())
        {
            var digit = digits.Pop();

            var digitString = (digit, juUnit) switch {
                (0, _) => string.Empty,
                (6, 2) => "ろっ",
                (8, 2) => "はっ",
                (1, 3) => "いっ",
                (8, 3) => "はっ",
                _ => Digits[digit - 1]
            };

            var manUnitString = (manUnit, juUnit) switch {
                (> 0, 0) => ManUnits[manUnit - 1],
                _ => string.Empty
            };

            var juUnitString = (digit, juUnit) switch {
                (_, 0) => string.Empty,
                (3, 2) => "びゃく",
                (6, 2) => "ぴゃく",
                (8, 2) => "ぴゃく",
                (3, 3) => "ぜん",
                _ => JuUnits[juUnit - 1]
            };

            builder.Append($"{digitString}{juUnitString}{manUnitString}");

            if (juUnit == 0)
            {
                juUnit = 3;
                --manUnit;
            }
            else
            {
                --juUnit;
            }
        }

        return builder.ToString();
    }

    public static readonly JapaneseNumberFormatter Instance = new();
}
