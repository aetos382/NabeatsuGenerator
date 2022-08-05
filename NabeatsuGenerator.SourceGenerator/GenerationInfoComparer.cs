using Microsoft.CodeAnalysis;

namespace NabeatsuGenerator.SourceGenerator;

internal sealed class GenerationInfoComparer :
    IEqualityComparer<GenerationInfo?>
{
    public bool Equals(
        GenerationInfo? x,
        GenerationInfo? y)
    {
        if (object.ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Equals(y);
    }

    public int GetHashCode(
        GenerationInfo? obj)
    {
        return obj?.GetHashCode() ?? 0;
    }

    public static GenerationInfoComparer Instance = new();

    private GenerationInfoComparer()
    {
    }
}