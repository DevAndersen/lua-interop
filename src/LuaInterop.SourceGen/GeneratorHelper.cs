using System.CodeDom.Compiler;

namespace LuaInterop.SourceGen;

internal static class GeneratorHelper
{
    private const string _typeNotFoundFromMetadataNameExceptionMessageTemplate = "Unable to find type from metadata name '{0}'";

    /// <summary>
    /// Determines if <paramref name="typeSymbol"/>, and all containing types, are either <c>public</c> or <c>internal</c>.
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <param name="problematicTypeSymbol"></param>
    /// <returns></returns>
    public static bool AreContainingTypesInaccessible(ITypeSymbol? typeSymbol, [NotNullWhen(true)] out ITypeSymbol? problematicTypeSymbol)
    {
        // Check if containing type is null (no problem).
        if (typeSymbol == null)
        {
            problematicTypeSymbol = null;
            return false;
        }

        // Check if containing type has unsupported accessibility (problem).
        if (typeSymbol is { DeclaredAccessibility: not (Accessibility.Public or Accessibility.Internal) })
        {
            problematicTypeSymbol = typeSymbol;
            return true;
        }

        // Check if containing type is itself contained within a type (potential problem).
        if (typeSymbol?.ContainingType != null)
        {
            return AreContainingTypesInaccessible(typeSymbol.ContainingType, out problematicTypeSymbol);
        }

        // Type has supported accessibility and is not contained within another type (no problem).
        problematicTypeSymbol = null;
        return false;
    }

    /// <summary>
    /// Provides null-or-whitespace check with <see cref="NotNullWhenAttribute"/> for .NET Standard 2.0.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static INamedTypeSymbol GetTypeByMetadataName(Compilation compilation, string typeMetadataName)
    {
        return compilation.GetTypeByMetadataName(typeMetadataName)
            ?? throw new Exception(string.Format(_typeNotFoundFromMetadataNameExceptionMessageTemplate, typeMetadataName)); // Todo: Emit diagnostics instead of throwing exception.
    }

    /// <summary>
    /// Generate <see cref="GeneratedCodeAttribute"/> attribute syntax.
    /// </summary>
    /// <returns></returns>
    public static AttributeSyntax GenerateGeneratedCodeAttributeAttribute()
    {
        string? name = typeof(LuaGenerator).Assembly.GetName().Name;
        string version = typeof(LuaGenerator).Assembly.GetName().Version.ToString();

        // Attribute, GeneratedCodeAttribute.
        return SF.Attribute(
            SF.IdentifierName(GeneratorConstants.GeneratedCodeAttributeAttributeGlobalFullName),
            SF.AttributeArgumentList([
                SF.AttributeArgument(
                    SF.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SF.Literal(name))),
                SF.AttributeArgument(
                    SF.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SF.Literal(version)))]));
    }

    public static bool TryGetAttributeValue<T>(string argumentName, ISymbol symbol, INamedTypeSymbol attributeTypeSymbol, [NotNullWhen(true)] out T? value)
    {
        AttributeData? matchingAttribute = GetAttributeData(symbol, attributeTypeSymbol);
        if (matchingAttribute == null)
        {
            value = default;
            return false;
        }

        return TryGetAttributeNamedArgument(matchingAttribute, argumentName, out value);
    }

    public static AttributeData? GetAttributeData(ISymbol symbol, INamedTypeSymbol attributeTypeSymbol)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Equals(attributeTypeSymbol, SymbolEqualityComparer.Default) == true);
    }

    private static bool TryGetAttributeNamedArgument<T>(AttributeData attributeData, string argumentName, out T? value)
    {
        KeyValuePair<string, TypedConstant> argument = attributeData.NamedArguments.FirstOrDefault(x => x.Key == argumentName);

        if (!argument.Equals(default) && argument.Value.Value is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }
}
