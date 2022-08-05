using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;

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
                var attributeSymbol = compilation.GetTypeByMetadataName("NabeatsuGenerator.NabeatsuAttribute")!;
                return attributeSymbol;
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
            .Select((tuple, _) => (
                MethodNode: tuple.Left.Left.Node,
                MethodSymbol: tuple.Left.Left.Symbol,
                NabeatuAttributeSymbol: tuple.Left.Right,
                IEnumerableOfStringSymbol: tuple.Right));

        context.RegisterImplementationSourceOutput(
            generationInfoProvider,
            static (context, source) => {

                var generationInfo = TryGetGenerationInfo(
                    context,
                    source.MethodNode,
                    source.MethodSymbol,
                    source.NabeatuAttributeSymbol,
                    source.IEnumerableOfStringSymbol);

                if (generationInfo is null)
                {
                    return;
                }

                var generatedSyntax = GenerateSyntax(generationInfo, context);

                var code = generatedSyntax.NormalizeWhitespace().ToFullString();

                var format = new SymbolDisplayFormat(
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    parameterOptions: SymbolDisplayParameterOptions.IncludeType);

                var typeName = source.MethodSymbol!.ContainingType!.ToDisplayString(format);
                var methodName = source.MethodSymbol.ToDisplayString(format);

                context.AddSource($"{typeName}.{methodName}.cs", code);

            });
    }

    static bool FilterSyntaxNode(
        SyntaxNode node,
        CancellationToken cancellationToken)
    {
        return
            node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
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
        SourceProductionContext context,
        MethodDeclarationSyntax methodSyntax,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol nabeatsuAttributeSymbol,
        INamedTypeSymbol enumerableOfStringSymbol)
    {
        var attrs = methodSymbol.GetAttributes();

        var nAttr = attrs
            .SingleOrDefault(
                a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, nabeatsuAttributeSymbol));

        if (nAttr is null)
        {
            return null;
        }

        var methodLocation = methodSyntax.GetLocation();

        if (!methodSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            context.ReportDiagnostic(
                Diagnostics.MethodMustBePartial(methodLocation));

            return null;
        }

        var attributeLocation = Location.Create(
            nAttr.ApplicationSyntaxReference.SyntaxTree,
            nAttr.ApplicationSyntaxReference.Span);

        var ctorArgs = nAttr.ConstructorArguments;

        var start = (int)ctorArgs[0].Value!;
        var end = (int)ctorArgs[1].Value!;

        if (start < 0)
        {
            context.ReportDiagnostic(
                Diagnostics.StartParameterValueMustBeGreaterThanOrEqualToZero(
                    attributeLocation,
                    start));

            return null;
        }

        if (end < start)
        {
            context.ReportDiagnostic(
                Diagnostics.EndParameterValueMustBeGreaterThanOrEqualToStartParameterValue(
                    attributeLocation,
                    end,
                    start));

            return null;
        }

        if (methodSymbol.ReturnType is not INamedTypeSymbol returnTypeSymbol ||
            !SymbolEqualityComparer.Default.Equals(enumerableOfStringSymbol, returnTypeSymbol))
        {
            context.ReportDiagnostic(
                Diagnostics.ReturnTypeOfMethodMustBeIEnumerableOfString(
                    methodLocation));

            return null;
        }

        var result = new GenerationInfo(
            methodSyntax,
            methodSymbol,
            start,
            end);

        return result;
    }

    static SyntaxNode GenerateSyntax(
        GenerationInfo info,
        SourceProductionContext context)
    {
        var yieldStatements = new List<YieldStatementSyntax>(info.End - info.Start + 1);

        for (var i = info.Start; i <= info.End; ++i)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var output = ConvertToNabeatsu(i);

            var yieldStatement = SyntaxFactory
                .YieldStatement(
                    SyntaxKind.YieldReturnStatement,
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(output)));

            yieldStatements.Add(yieldStatement);
        }

        var sourceMethodNode = info.MethodNode;

        var returnTypeName = info.MethodSymbol.ReturnType.ToDisplayString(
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters));

        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseName(returnTypeName), sourceMethodNode.Identifier)
            .WithModifiers(sourceMethodNode.Modifiers)
            .WithBody(SyntaxFactory.Block(yieldStatements));

        var sourceTypeNode = (TypeDeclarationSyntax)sourceMethodNode.Parent!;
        var topSourceTypeNode = sourceTypeNode;

        TypeDeclarationSyntax topResultTypeNode = SyntaxFactory
            .ClassDeclaration(sourceTypeNode.Identifier)
            .WithModifiers(sourceTypeNode.Modifiers)
            .WithMembers(
                SyntaxFactory.List(
                    new MemberDeclarationSyntax[] { method }));

        sourceTypeNode = sourceTypeNode.Parent as TypeDeclarationSyntax;
        while (sourceTypeNode is not null)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var parentTypeNode = SyntaxFactory
                .TypeDeclaration(sourceTypeNode.Kind(), sourceTypeNode.Identifier)
                .WithModifiers(sourceTypeNode.Modifiers)
                .WithMembers(
                    SyntaxFactory.List(
                        new MemberDeclarationSyntax[] { topResultTypeNode }));

            topResultTypeNode = parentTypeNode;
            topSourceTypeNode = sourceTypeNode;

            sourceTypeNode = sourceTypeNode.Parent as TypeDeclarationSyntax;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var att = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        var version = att.InformationalVersion;

        topResultTypeNode = topResultTypeNode
            .WithAttribute(
                typeof(GeneratedCodeAttribute),
                new (string?, object?) [] {
                    (null, typeof(Generator).FullName),
                    (null, version)
                },
                cancellationToken: context.CancellationToken);

        SyntaxNode resultNode = topResultTypeNode;

        var nsNames = new Stack<string>();

        var sourceNsNode = (BaseNamespaceDeclarationSyntax?)topSourceTypeNode.Parent;
        while (sourceNsNode is not null)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

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
                        new MemberDeclarationSyntax[] { topResultTypeNode }));

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
            return string.Format(JapaneseNumberFormatter.Default, "{0:H}", value);
        }

        return standardString;
    }

    private void GenerateInitialCode(
        IncrementalGeneratorPostInitializationContext context)
    {
    }
}
