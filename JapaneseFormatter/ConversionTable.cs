namespace JapaneseFormatter;

internal class ConversionTable
{
    public IReadOnlyList<string> Digits { get; }

    public IReadOnlyList<string> JuUnits { get; }

    public IReadOnlyList<string> ManUnits { get; }

    public ConversionTable(
        IReadOnlyList<string> digits,
        IReadOnlyList<string> juUnits,
        IReadOnlyList<string> manUnits)
    {
        this.Digits = digits;
        this.JuUnits = juUnits;
        this.ManUnits = manUnits;
    }

    public static ConversionTable Hiragana = new ConversionTable(
        HiraganaDigits, HiraganaJuUnits, HiraganaManUnits);

    private static readonly string[] HiraganaDigits = {
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

    private static readonly string[] HiraganaManUnits = {
        "いち",
        "まん",
        "おく"
    };

    private static readonly string[] HiraganaJuUnits = {
        "いち",
        "じゅう",
        "ひゃく",
        "せん"
    };
}
