using System.Globalization;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using JapaneseFormatter;

namespace NabeatsuGenerator.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class Generator :
    IIncrementalGenerator
{
    public void Initialize(
        IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(this.GenerateInitialCode);

        var nabeatsuAttributeProvider = context.CompilationProvider
            .Select((compilation, _) => {
                return compilation.GetTypeByMetadataName("NabeatsuGenerator.NabeatsuAttribute")!;
            })
            .WithComparer(SymbolEqualityComparer.Default);

        var enumerableOfStringProvider = context.CompilationProvider
            .Select((compilation, _) => {
                var ieSymbol = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
                var stringSymbol = compilation.GetSpecialType(SpecialType.System_String);

                return ieSymbol.Construct(stringSymbol);
            })
            .WithComparer(SymbolEqualityComparer.Default);

        var generationInfoProvider = context.SyntaxProvider
            .CreateSyntaxProvider(FilterSyntaxNode, TransformSyntaxNode)
            .WithComparer(MethodNodeAndSymbolComparer.Instance)
            .Combine(nabeatsuAttributeProvider)
            .Combine(enumerableOfStringProvider)
            .Select(static (tuple, _) => TryGetGenerationInfo(
                tuple.Left.Left.Node,
                tuple.Left.Left.Symbol,
                tuple.Left.Right,
                tuple.Right))
            .Where(static info => info is not null)
            .WithComparer(GenerationInfoComparer.Instance);

        context.RegisterImplementationSourceOutput(
            generationInfoProvider,
            static (context, source) => {

                var generatedSyntax = GenerateSyntax(source);
                var code = generatedSyntax.NormalizeWhitespace().ToFullString();

                context.AddSource("foo.cs", code);

            });
    }

    static bool FilterSyntaxNode(
        SyntaxNode node,
        CancellationToken cancellationToken)
    {
        return
            node is MethodDeclarationSyntax mds &&
            mds.AttributeLists.Count > 0 &&
            mds.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    static (MethodDeclarationSyntax Node, IMethodSymbol Symbol) TransformSyntaxNode(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var node = (MethodDeclarationSyntax)context.Node;
        var symbol = (IMethodSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken)!;

        return (node, symbol);
    }

    static GenerationInfo? TryGetGenerationInfo(
        MethodDeclarationSyntax syntaxNode,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol nabeatsuAttributeSymnbol,
        INamedTypeSymbol enumerableOfStringSymbol)
    {
        var attrs = methodSymbol.GetAttributes();

        var nAttr = attrs
            .SingleOrDefault(
                a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, nabeatsuAttributeSymnbol));

        if (nAttr is null)
        {
            return null;
        }

        var ctorArgs = nAttr.ConstructorArguments;

        // TODO: warning
        var result = new GenerationInfo {
            Node = syntaxNode,
            Method = methodSymbol,
            Start = (int)ctorArgs[0].Value!,
            End = (int)ctorArgs[1].Value!
        };

        // TODO: warning
        if (methodSymbol.ReturnType is not INamedTypeSymbol returnTypeSymbol)
        {
            return null;
        }

        // TODO: warning
        if (!SymbolEqualityComparer.Default.Equals(enumerableOfStringSymbol, returnTypeSymbol))
        {
            return null;
        }

        return result;
    }

    static SyntaxNode GenerateSyntax(
        GenerationInfo info)
    {
        var yieldStatements = new List<YieldStatementSyntax>(info.End - info.Start + 1);

        for (var i = info.Start; i <= info.End; ++i)
        {
            var output = ConvertToNabeatsu(i);

            var yieldStatement = SyntaxFactory
                .YieldStatement(
                    SyntaxKind.YieldReturnStatement,
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(output)));

            yieldStatements.Add(yieldStatement);
        }

        var sourceMethodNode = info.Node;

        var method = SyntaxFactory
            .MethodDeclaration(sourceMethodNode.ReturnType, sourceMethodNode.Identifier)
            .WithModifiers(sourceMethodNode.Modifiers)
            .WithBody(SyntaxFactory.Block(yieldStatements));

        var sourceTypeNode = (TypeDeclarationSyntax)sourceMethodNode.Parent!;

        SyntaxNode resultNode = SyntaxFactory
            .ClassDeclaration(sourceTypeNode.Identifier)
            .WithModifiers(sourceTypeNode.Modifiers)
            .WithMembers(
                SyntaxFactory.List(
                    new MemberDeclarationSyntax[] { method }));

        var topSourceTypeNode = sourceTypeNode;

        sourceTypeNode = sourceTypeNode.Parent as TypeDeclarationSyntax;
        while (sourceTypeNode is not null)
        {
            var parentTypeDecl = SyntaxFactory
                .TypeDeclaration(sourceTypeNode.Kind(), sourceTypeNode.Identifier)
                .WithModifiers(sourceTypeNode.Modifiers)
                .WithMembers(
                    SyntaxFactory.List(
                        new MemberDeclarationSyntax[] { (TypeDeclarationSyntax)resultNode }));

            resultNode = parentTypeDecl;

            topSourceTypeNode = sourceTypeNode;
            sourceTypeNode = sourceTypeNode.Parent as TypeDeclarationSyntax;
        }

        var nsNames = new Stack<string>();

        var sourceNsNode = (BaseNamespaceDeclarationSyntax?)topSourceTypeNode.Parent;
        while (sourceNsNode is not null)
        {
            nsNames.Push(sourceNsNode.Name.ToString());
            sourceNsNode = sourceNsNode.Parent as BaseNamespaceDeclarationSyntax;
        }

        if (nsNames.Any())
        {
            var nsName = SyntaxFactory.ParseName(string.Join(".", nsNames));

            var resultNsNode = SyntaxFactory
                .NamespaceDeclaration(nsName)
                .WithMembers(
                    SyntaxFactory.List(
                        new MemberDeclarationSyntax[] { (TypeDeclarationSyntax)resultNode }));

            resultNode = resultNsNode;
        }

        var autoGeneratedCommentTrivia = SyntaxFactory.Comment("// <auto-generated/>");
        return resultNode.WithLeadingTrivia(autoGeneratedCommentTrivia);
    }

    private static string ConvertToNabeatsu(
        int value)
    {
        var standardString = value.ToString(CultureInfo.InvariantCulture);

        if (value % 3 == 0 || standardString.Contains("3"))
        {
            return string.Format(JapaneseNumberFormatter.Instance, "{0:H}", value);
        }

        return standardString;
    }

    private void GenerateInitialCode(
        IncrementalGeneratorPostInitializationContext context)
    {
    }
}
