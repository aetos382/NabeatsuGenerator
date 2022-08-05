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

        var conversionTable = format switch {
            Formats.Hiragana => ConversionTable.Hiragana,
            _ => throw new FormatException()
        };

        if (arg is not int value)
        {
            throw new NotSupportedException();
        }

        return ToJapaneseString(value, conversionTable);
    }

    private static string ToJapaneseString(
        int value,
        IConversionTable table)
    {
        var result = table.SpecialFormat(value);

        if (result is not null)
        {
            return result;
        }

        var builder = new StringBuilder();

        var digits = new Stack<int>();

        while (value > 0)
        {
            digits.Push(value % 10);
            value /= 10;
        }

        var length = digits.Count;

        var manUnit = (length - 1) / 4;
        var juUnit = (length - 1 ) % 4;

        while (digits.Any())
        {
            var digit = digits.Pop();

            var digitString = table.GetDigit(digit, juUnit, manUnit);
            var juUnitString = table.GetJuUnit(digit, juUnit, manUnit);
            var manUnitString = table.GetManUnit(digit, juUnit, manUnit);

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

    public static readonly JapaneseNumberFormatter Default = new();
}
