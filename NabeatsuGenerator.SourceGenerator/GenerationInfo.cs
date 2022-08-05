using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NabeatsuGenerator.SourceGenerator;

internal sealed record GenerationInfo(
    MethodDeclarationSyntax MethodNode,
    IMethodSymbol MethodSymbol,
    int Start,
    int End)
{
    public bool Equals(
        GenerationInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (object.ReferenceEquals(this, other))
        {
            return true;
        }

        return
            this.MethodNode.IsEquivalentTo(other.MethodNode) &&
            SymbolEqualityComparer.Default.Equals(this.MethodSymbol, other.MethodSymbol) &&
            this.Start == other.Start &&
            this.End == other.End;
    }

    public override int GetHashCode()
    {
        return
            HashCode.Combine(
                this.MethodNode,
                SymbolEqualityComparer.Default.GetHashCode(this.MethodSymbol),
                this.Start,
                this.End);
    }
}
