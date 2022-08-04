using Microsoft.CodeAnalysis;

namespace NabeatsuGenerator.SourceGenerator;

internal class KnownTypes
{
    public KnownTypes(
        Compilation compilation)
    {
        this.Compilation = compilation;

        this.String = compilation.GetSpecialType(SpecialType.System_String);
        this.IEnumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);

        this.NabeatsuAttribute = GetTypeSymbol("NabeatsuGenerator.NabeatsuAttribute")!;
    }

    public Compilation Compilation { get; }

    public INamedTypeSymbol String { get; }

    public INamedTypeSymbol IEnumerableOfT { get; }

    public INamedTypeSymbol NabeatsuAttribute { get; }

    public INamedTypeSymbol? GetTypeSymbol(
        string typeName)
    {
        return this.Compilation.GetTypeByMetadataName(typeName);
    }

    public bool IsString(
    ITypeSymbol typeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(typeSymbol, this.String);
    }

    public bool IsIEnumerableOfT(
        ITypeSymbol typeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(typeSymbol, this.IEnumerableOfT);
    }

    public bool IsNabeatsuAttribute(
        ITypeSymbol typeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(typeSymbol, this.NabeatsuAttribute);
    }
}
