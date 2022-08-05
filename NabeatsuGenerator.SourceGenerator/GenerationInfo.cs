using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NabeatsuGenerator.SourceGenerator;

internal sealed record GenerationInfo(
    MethodDeclarationSyntax MethodNode,
    IMethodSymbol MethodSymbol,
    int Start,
    int End);