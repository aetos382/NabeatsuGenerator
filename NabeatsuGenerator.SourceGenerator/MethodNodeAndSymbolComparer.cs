using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NabeatsuGenerator.SourceGenerator;

internal sealed class MethodNodeAndSymbolComparer :
    IEqualityComparer<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>
{
    public bool Equals(
        (MethodDeclarationSyntax Node, IMethodSymbol Symbol) x,
        (MethodDeclarationSyntax Node, IMethodSymbol Symbol) y)
    {
        return
            x.Node.IsEquivalentTo(y.Node) &&
            SymbolEqualityComparer.Default.Equals(x.Symbol, y.Symbol);
    }

    public int GetHashCode(
        (MethodDeclarationSyntax Node, IMethodSymbol Symbol) obj)
    {
        unchecked
        {
            return
                (obj.Node.GetHashCode() * 397) ^
                SymbolEqualityComparer.Default.GetHashCode(obj.Symbol);
        }
    }

    public static readonly MethodNodeAndSymbolComparer Instance = new();

    private MethodNodeAndSymbolComparer()
    {
    }
}