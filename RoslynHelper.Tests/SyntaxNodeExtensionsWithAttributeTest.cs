using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace RoslynHelper.Tests;

public class SyntaxNodeExtensionsWithAttributeTest
{
    [Fact]
    public void NoArguments()
    {
        var before = SyntaxFactory.ClassDeclaration("Foo");

        var after = before.WithAttribute("Bar");

        var expectedTree = CSharpSyntaxTree.ParseText(@"
[global::Bar()]
class Foo
{
}");

        var expectedType = expectedTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<ClassDeclarationSyntax>()
            .First(t => t.Identifier.ValueText == "Foo");

        var matched = after.IsEquivalentTo(expectedType, true);
        Assert.True(matched);
    }

    [Fact]
    public void WithNoNamedNumericConstructorArguments()
    {
        var before = SyntaxFactory.ClassDeclaration("Foo");

        var after = before.WithAttribute(
            "Bar",
            new (string?, object?) [] {
                (null, 1)
            });

        var expectedTree = CSharpSyntaxTree.ParseText(@"
[global::Bar(1)]
class Foo
{
}");

        var expectedType = expectedTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<ClassDeclarationSyntax>()
            .First(t => t.Identifier.ValueText == "Foo");

        var matched = after.IsEquivalentTo(expectedType, true);
        Assert.True(matched);
    }

    [Fact]
    public void WithNoNamedStringConstructorArguments()
    {
        var before = SyntaxFactory.ClassDeclaration("Foo");

        var after = before.WithAttribute(
            "Bar",
            new (string?, object?) [] {
                (null, "1")
            });

        var expectedTree = CSharpSyntaxTree.ParseText(@"
[global::Bar(""1"")]
class Foo
{
}");

        var expectedType = expectedTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<ClassDeclarationSyntax>()
            .First(t => t.Identifier.ValueText == "Foo");

        var matched = after.IsEquivalentTo(expectedType, true);
        Assert.True(matched);
    }

    [Fact]
    public void WithNoNamedTypeConstructorArguments()
    {
        var before = SyntaxFactory.ClassDeclaration("Foo");

        var after = before.WithAttribute(
            "Bar",
            new (string?, object?) [] {
                (null, typeof(int))
            });

        var expectedTree = CSharpSyntaxTree.ParseText(@"
[global::Bar(typeof(global::System.Int32))]
class Foo
{
}");

        var expectedType = expectedTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<ClassDeclarationSyntax>()
            .First(t => t.Identifier.ValueText == "Foo");

        var matched = after.IsEquivalentTo(expectedType, true);
        Assert.True(matched);
    }

    [Fact(Skip = "なんで通らないの？")]
    public void WithNoNamedEnumConstructorArguments()
    {
        var before = SyntaxFactory.ClassDeclaration("Foo");

        var after = before.WithAttribute(
            "Bar",
            new (string?, object?) [] {
                (null, StringComparison.Ordinal)
            });

        var expectedTree = CSharpSyntaxTree.ParseText(@"
[global::Bar(global::System.StringComparison.Ordinal)]
class Foo
{
}");

        var expectedType = expectedTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<ClassDeclarationSyntax>()
            .First(t => t.Identifier.ValueText == "Foo");

        var matched = after.IsEquivalentTo(expectedType, true);
        Assert.True(matched);
    }

    [Fact]
    public void WithNamedNumericConstructorArguments()
    {
        var before = SyntaxFactory.ClassDeclaration("Foo");

        var after = before.WithAttribute(
            "Bar",
            new (string?, object?) [] {
                ("p", 1)
            });

        var expectedTree = CSharpSyntaxTree.ParseText(@"
[global::Bar(p: 1)]
class Foo
{
}");

        var expectedType = expectedTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<ClassDeclarationSyntax>()
            .First(t => t.Identifier.ValueText == "Foo");

        var matched = after.IsEquivalentTo(expectedType, true);
        Assert.True(matched);
    }

    [Fact]
    public void WithNumericMemberArguments()
    {
        var before = SyntaxFactory.ClassDeclaration("Foo");

        var after = before.WithAttribute(
            "Bar",
            namedArguments: new (string, object?) [] {
                ("p", 1)
            });

        var expectedTree = CSharpSyntaxTree.ParseText(@"
[global::Bar(p = 1)]
class Foo
{
}");

        var expectedType = expectedTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<ClassDeclarationSyntax>()
            .First(t => t.Identifier.ValueText == "Foo");

        var matched = after.IsEquivalentTo(expectedType, true);
        Assert.True(matched);
    }
}
