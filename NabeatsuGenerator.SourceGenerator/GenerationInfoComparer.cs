using Microsoft.CodeAnalysis;

namespace NabeatsuGenerator.SourceGenerator;

internal sealed class GenerationInfoComparer :
    IEqualityComparer<GenerationInfo>
{
    public bool Equals(
        GenerationInfo? x,
        GenerationInfo? y)
    {
        if (object.ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        return
            x.Node.IsEquivalentTo(y.Node) &&
            SymbolEqualityComparer.Default.Equals(x.Method, y.Method) &&
            x.Start == y.Start &&
            x.End == y.End;
    }

    public int GetHashCode(GenerationInfo obj)
    {
        unchecked
        {
            var hashCode = obj.Node.GetHashCode();
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(obj.Method);
            hashCode = (hashCode * 397) ^ obj.Start;
            hashCode = (hashCode * 397) ^ obj.End;
            return hashCode;
        }
    }

    public static GenerationInfoComparer Instance = new();

    private GenerationInfoComparer()
    {
    }
}