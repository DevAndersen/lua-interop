using Microsoft.CodeAnalysis;

namespace LuaInterop.SourceGen;

internal static class Extensions
{
    extension(TypeDictionary dictionary)
    {
        public INamedTypeSymbol GetOrThrow(SpecialType specialType)
        {
            if (dictionary.TryGetValue(specialType, out INamedTypeSymbol? type))
            {
                return type;
            }

            throw new KeyNotFoundException($"Failed to find special type '{specialType}' in type dictionary.");
        }

        public string GetNameOrThrow(SpecialType specialType)
        {
            return dictionary.GetOrThrow(specialType).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }

    extension(ITypeSymbol typeSymbol)
    {
        /// <summary>
        /// Returns the fully qualified name of <paramref name="typeSymbol"/>.
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }
}
