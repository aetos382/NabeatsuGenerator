using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynHelper;

public static partial class NameExtensions
{
    public static NameSyntax WithGlobalPrefix(
        this NameSyntax name)
    {
        var globalToken = SyntaxFactory.Token(SyntaxKind.GlobalKeyword);
        var globalName = SyntaxFactory.IdentifierName(globalToken);

        if (name is QualifiedNameSyntax qName)
        {
            var rightNames = new Stack<SimpleNameSyntax>();

            NameSyntax leftmostName = qName;

            while (leftmostName is QualifiedNameSyntax q)
            {
                leftmostName = q.Left;
                rightNames.Push(q.Right);
            }

            NameSyntax fullName = leftmostName switch {
                SimpleNameSyntax s => SyntaxFactory.AliasQualifiedName(globalName, s),
                var other => other
            };

            while (rightNames.Any())
            {
                fullName = SyntaxFactory.QualifiedName(
                    fullName, rightNames.Pop()!);
            }

            return fullName;
        }
        else if (name is SimpleNameSyntax sName)
        {
            NameSyntax fullName = SyntaxFactory.AliasQualifiedName(
                globalName, sName);

            return fullName;
        }

        return name;
    }
}
