using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace RoslynHelper.Tests;

public class NameExtensionsWithGlobalPrefixTest
{
    [Fact]
    public void SimpleNameTest()
    {
        var nameSyntax = SyntaxFactory.ParseTypeName("Foo") as NameSyntax;
        var withGlobal = nameSyntax!.WithGlobalPrefix();
        var name = withGlobal.ToString();

        Assert.Equal("global::Foo", name);
    }

    [Fact]
    public void QualifiedNameTest()
    {
        var nameSyntax = SyntaxFactory.ParseTypeName("Foo.Bar") as NameSyntax;
        var withGlobal = nameSyntax!.WithGlobalPrefix();
        var name = withGlobal.ToString();

        Assert.Equal("global::Foo.Bar", name);
    }
}