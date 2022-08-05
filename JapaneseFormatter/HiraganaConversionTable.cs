namespace JapaneseFormatter;

internal class HiraganaConversionTable :
    IConversionTable
{
    private HiraganaConversionTable()
    {
    }

    public static readonly HiraganaConversionTable Default = new();

    public string? SpecialFormat(
        int number)
    {
        return number switch {
            0 => "ぜろ",
            _ => null
        };
    }

    public string GetDigit(
        int digit,
        int juUnit,
        int manUnit)
    {
        return (digit, juUnit, manUnit) switch {
            (0, _, _) => string.Empty,
            (1, not 0, 0) => string.Empty,
            (6, 2, _) => "ろっ",
            (8, 2, _) => "はっ",
            (1, 3, 1 or 2) => "いっ",
            (1, 0, 3 or 4 or 8 or 9 or 10) => "いっ",
            (8, 3, _) => "はっ",
            (8, 0, 3 or 4 or 8 or 9 or 10) => "はっ",
            _ => _digits[digit]
        };
    }

    public string GetJuUnit(
        int digit,
        int juUnit,
        int manUnit)
    {
        return (digit, juUnit, manUnit) switch {
            (0, _, _) => string.Empty,
            (_, 0, _) => string.Empty,
            (3, 2, _) => "びゃく",
            (6 or 8, 2, _) => "ぴゃく",
            (3, 3, _) => "ぜん",
            _ => _juUnits[juUnit]
        };
    }

    public string GetManUnit(
        int digit,
        int juUnit,
        int manUnit)
    {
        return (digit, juUnit, manUnit) switch {
            (_, not 0, _) => string.Empty,
            (_, 0, not 0) => _manUnits[manUnit],
            (0, _, _) => string.Empty,
            (_, _, 0) => string.Empty
        };
    }

    private static readonly string[] _digits = {
        "ぜろ",
        "いち",
        "に",
        "さん",
        "よん",
        "ご",
        "ろく",
        "なな",
        "はち",
        "きゅう"
    };

    private static readonly string[] _juUnits = {
        "いち",
        "じゅう",
        "ひゃく",
        "せん"
    };

    private static readonly string[] _manUnits = {
        "いち",
        "まん",
        "おく",
        "ちょう",
        "けい",
        "がい",
        "じょう",
        "じょ"
    };
}
