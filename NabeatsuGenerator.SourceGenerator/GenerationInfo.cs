
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NabeatsuGenerator.SourceGenerator;

internal sealed class GenerationInfo
{
    public MethodDeclarationSyntax Node { get; set; }

    public IMethodSymbol Method { get; set; }

    public int Start { get; set; }

    public int End { get; set; }
}
