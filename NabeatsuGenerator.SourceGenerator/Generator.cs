using System.Globalization;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NabeatsuGenerator.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class Generator :
    IIncrementalGenerator
{
    public void Initialize(
        IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(this.GenerateInitialCode);

        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            FilterSyntaxNode, TransformSyntaxNode);

        var methods = syntaxProvider
            .Combine(context.CompilationProvider)
            .Select(static (tuple, _) => (
                tuple.Left.Node,
                tuple.Left.Symbol,
                Compilation: tuple.Right,
                KnownTypes: new KnownTypes(tuple.Right)))
            .Select(static (tuple, _) => TryGetGenerationInfo(tuple.Node, tuple.Symbol, tuple.KnownTypes))
            .Where(static info => info is not null);

        context.RegisterImplementationSourceOutput(
            methods,
            static (context, source) => {

                var generatedSyntax = GenerateSyntax(source);
                var code = generatedSyntax.NormalizeWhitespace().ToFullString();

                context.AddSource("foo.cs", code);

            });

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
            KnownTypes knownTypes)
        {
            var attrs = methodSymbol.GetAttributes();

            var nAttr = attrs
                .SingleOrDefault(a => knownTypes.IsNabeatsuAttribute(a.AttributeClass!));

            if (nAttr is null)
            {
                return null;
            }

            var ctorArgs = nAttr.ConstructorArguments;

            // TODO: warning
            var result = new GenerationInfo {
                Node = syntaxNode,
                Method = methodSymbol,
                NabeatsuAttribute = nAttr,
                Start = (int)ctorArgs[0].Value!,
                End = (int)ctorArgs[1].Value!
            };

            // TODO: warning
            if (methodSymbol.ReturnType is not INamedTypeSymbol returnTypeSymbol)
            {
                return null;
            }

            // TODO: warning
            if (!returnTypeSymbol.IsGenericType ||
                !knownTypes.IsIEnumerableOfT(returnTypeSymbol.ConstructedFrom) ||
                !knownTypes.IsString(returnTypeSymbol.TypeArguments[0]))
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
                var output = ToNabeatsuString(i);

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
    }

    private static readonly string[] Digits = {
        "いち",
        "に",
        "さん",
        "よん",
        "ご",
        "ろく",
        "なな",
        "はち",
        "きう"
    };

    private static readonly string[] ManUnits = {
        "まん",
        "おく"
    };
    
    private static readonly string[] JuUnits = {
        "じゅう",
        "ひゃく",
        "せん"
    };

    static string ToNabeatsuString(
        int value)
    {
        if (value == 0)
        {
            return "ぜろ";
        }

        var wiseString = value.ToString(CultureInfo.InvariantCulture);
        var isAho = value % 3 == 0 || wiseString.Contains("3");

        if (!isAho)
        {
            return wiseString;
        }

        var builder = new StringBuilder();

        var length = wiseString.Length;

        var manUnit = length switch {
            10 or 9 => 2,
            8 or 7 or 6 or 5 => 1,
            4 or 3 or 2 or 1 => 0
        };

        var juUnit = length switch {
            8 or 4 => 3,
            7 or 3 => 2,
            10 or 6 or 2 => 1,
            9 or 5 or 1 => 0
        };

        for (int i = 0; i < length; ++i)
        {
            var digit = wiseString[i] - '0';

            var digitString = (digit, juUnit) switch {
                (0, _) => string.Empty,
                (6, 2) => "ろっ",
                (8, 2) => "はっ",
                (1, 3) => "いっ",
                (8, 3) => "はっ",
                _ => Digits[digit - 1]
            };

            var manUnitString = (manUnit, juUnit) switch {
                (> 0, 0) => ManUnits[manUnit - 1],
                _ => string.Empty
            };
            
            var juUnitString = (digit, juUnit) switch {
                (_, 0) => string.Empty,
                (3, 2) => "びゃく",
                (6, 2) => "ぴゃく",
                (8, 2) => "ぴゃく",
                (3, 3) => "ぜん",
                _ => JuUnits[juUnit - 1]
            };

            builder.Append($"{digitString}{juUnitString}{manUnitString}");

            if (juUnit == 0)
            {
                juUnit = 3;
                --manUnit;
            }
            else
            {
                --juUnit;
            }
        }

        return builder.ToString();
    }

    private void GenerateInitialCode(
        IncrementalGeneratorPostInitializationContext context)
    {
    }

    class GenerationInfo
    {
        public MethodDeclarationSyntax Node { get; set; }

        public IMethodSymbol Method { get; set; }

        public AttributeData NabeatsuAttribute { get; set; }

        public int Start { get; set; }

        public int End { get; set; }
    }
}
