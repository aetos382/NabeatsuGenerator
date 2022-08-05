using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynHelper;

public static partial class SyntaxNodeExtensions
{
    public static TypeDeclarationSyntax WithAttribute(
        this TypeDeclarationSyntax syntax,
        Type attributeType,
        IReadOnlyList<(string? ParameterName, object? Value)>? constructorArguments = null,
        IReadOnlyList<(string MemberName, object? Value)>? namedArguments = null,
        CancellationToken cancellationToken = default)
    {
        if (attributeType is null)
        {
            throw new ArgumentNullException(nameof(attributeType));
        }

        return WithAttribute(
            syntax,
            $"global::{attributeType.FullName}",
            constructorArguments,
            namedArguments,
            cancellationToken);
    }

    public static TypeDeclarationSyntax WithAttribute(
        this TypeDeclarationSyntax syntax,
        INamedTypeSymbol attributeTypeSymbol,
        IReadOnlyList<(string? ParameterName, object? Value)>? constructorArguments = null,
        IReadOnlyList<(string MemberName, object? Value)>? namedArguments = null,
        CancellationToken cancellationToken = default)
    {
        if (attributeTypeSymbol is null)
        {
            throw new ArgumentNullException(nameof(attributeTypeSymbol));
        }

        var name = attributeTypeSymbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat);

        return WithAttribute(
            syntax,
            name,
            constructorArguments,
            namedArguments,
            cancellationToken);
    }

    public static TypeDeclarationSyntax WithAttribute(
        this TypeDeclarationSyntax syntax,
        string attributeTypeName,
        IReadOnlyList<(string? ParameterName, object? Value)>? constructorArguments = null,
        IReadOnlyList<(string MemberName, object? Value)>? namedArguments = null,
        CancellationToken cancellationToken = default)
    {
        if (syntax is null)
        {
            throw new ArgumentNullException(nameof(syntax));
        }

        if (attributeTypeName is null)
        {
            throw new ArgumentNullException(nameof(attributeTypeName));
        }

        var list = CreateAttributeLists(
            attributeTypeName,
            constructorArguments,
            namedArguments,
            cancellationToken);

        syntax = syntax.WithAttributeLists(list);
        return syntax;
    }

    private static SyntaxList<AttributeListSyntax> CreateAttributeLists(
        string attributeTypeName,
        IReadOnlyList<(string? ParameterName, object? Value)>? constructorArguments,
        IReadOnlyList<(string MemberName, object? Value)>? namedArguments,
        CancellationToken cancellationToken)
    {
        var totalCount =
            constructorArguments?.Count ?? 0 +
            namedArguments?.Count ?? 0;

        var attributeArguments = new List<AttributeArgumentSyntax>(totalCount);

        if (constructorArguments is not null)
        {
            foreach (var (name, value) in constructorArguments)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression = ValueToExpression(value);

                var nameSyntax = name switch {
                    null => default,
                    _ => SyntaxFactory.NameColon(name)
                };

                var argument = SyntaxFactory.AttributeArgument(
                    default, nameSyntax, expression);

                attributeArguments.Add(argument);
            }
        }

        if (namedArguments is not null)
        {
            foreach (var (name, value) in namedArguments)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var expression = ValueToExpression(value);

                var nameSyntax = name switch {
                    null => default,
                    _ => SyntaxFactory.NameEquals(name)
                };

                var argument = SyntaxFactory.AttributeArgument(
                    nameSyntax, default, expression);

                attributeArguments.Add(argument);
            }
        }

        var list = SyntaxFactory.List(
            new[] {
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SeparatedList(
                        new[] {
                            SyntaxFactory.Attribute(
                                SyntaxFactory.ParseName(attributeTypeName).WithGlobalPrefix(),
                                SyntaxFactory.AttributeArgumentList(
                                    SyntaxFactory.SeparatedList(
                                        attributeArguments)))
                        }))
            });

        return list;
    }

    private static ExpressionSyntax ValueToExpression(
        object? value)
    {
        ExpressionSyntax expression;

        if (value is Type t)
        {
            var typeName = SyntaxFactory.ParseTypeName(t.FullName!) as NameSyntax;
            expression = SyntaxFactory.TypeOfExpression(typeName!.WithGlobalPrefix());
        }
        else if (value is Enum e)
        {
            var enumType = e.GetType();

            expression = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ParseName(enumType.FullName!).WithGlobalPrefix(),
                SyntaxFactory.IdentifierName(Enum.GetName(enumType, e)!));
        }
        else
        {
            var (kind, token) = value switch {
                null => (SyntaxKind.NullLiteralExpression, SyntaxFactory.Token(SyntaxKind.NullKeyword)),
                byte v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                sbyte v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                short v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                ushort v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                int v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                uint v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                long v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                ulong v => (SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                string v => (SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(v)),
                char v => (SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(v)),
                bool b => b switch {
                    true => (SyntaxKind.TrueLiteralExpression, SyntaxFactory.Token(SyntaxKind.TrueKeyword)),
                    false => (SyntaxKind.FalseLiteralExpression, SyntaxFactory.Token(SyntaxKind.FalseKeyword))
                },
                _ => throw new NotSupportedException()
            };

            expression = SyntaxFactory.LiteralExpression(kind, token);
        }

        return expression;
    }
}
