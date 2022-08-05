using Microsoft.CodeAnalysis;

using NabeatsuGenerator.SourceGenerator.Properties;

namespace NabeatsuGenerator.SourceGenerator;

internal static class Diagnostics
{
    private static LocalizableString CreateResourceString(
        string name)
    {
        return new LocalizableResourceString(
            name,
            Resources.ResourceManager,
            typeof(Resources));
    }

    private static readonly DiagnosticDescriptor _aho001 =
        new DiagnosticDescriptor(
            "AHO001",
            CreateResourceString(nameof(Resources.AHO001Title)),
            CreateResourceString(nameof(Resources.AHO001Format)),
            "None",
            DiagnosticSeverity.Error,
            true);

    private static readonly DiagnosticDescriptor _aho002 =
        new DiagnosticDescriptor(
            "AHO002",
            CreateResourceString(nameof(Resources.AHO002Title)),
            CreateResourceString(nameof(Resources.AHO002Format)),
            "None",
            DiagnosticSeverity.Error,
            true);

    private static readonly DiagnosticDescriptor _aho003 =
        new DiagnosticDescriptor(
            "AHO003",
            CreateResourceString(nameof(Resources.AHO003Title)),
            CreateResourceString(nameof(Resources.AHO003Format)),
            "None",
            DiagnosticSeverity.Error,
            true);

    private static readonly DiagnosticDescriptor _aho004 =
        new DiagnosticDescriptor(
            "AHO004",
            CreateResourceString(nameof(Resources.AHO004Title)),
            CreateResourceString(nameof(Resources.AHO004Format)),
            "None",
            DiagnosticSeverity.Error,
            true);

    public static Diagnostic MethodMustBePartial(
        Location location)
    {
        return Diagnostic.Create(
            _aho001,
            location);
    }

    public static Diagnostic StartParameterValueMustBeGreaterThanOrEqualToZero(
        Location location,
        int value)
    {
        return Diagnostic.Create(
            _aho002,
            location,
            value);
    }

    public static Diagnostic EndParameterValueMustBeGreaterThanOrEqualToStartParameterValue(
        Location location,
        int endValue,
        int startValue)
    {
        return Diagnostic.Create(
            _aho003,
            location,
            endValue,
            startValue);
    }

    public static Diagnostic ReturnTypeOfMethodMustBeIEnumerableOfString(
        Location location)
    {
        return Diagnostic.Create(
            _aho004,
            location);
    }
}
