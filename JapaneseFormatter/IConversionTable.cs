namespace JapaneseFormatter;

internal interface IConversionTable
{
    string? SpecialFormat(
        int number);

    string GetDigit(
        int digit,
        int juUnit,
        int manUnit);

    string GetJuUnit(
        int digit,
        int juUnit,
        int manUnit);

    string GetManUnit(
        int digit,
        int juUnit,
        int manUnit);
}
