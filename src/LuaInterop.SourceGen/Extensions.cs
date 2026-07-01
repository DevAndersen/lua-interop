using Microsoft.CodeAnalysis;

namespace LuaInterop.SourceGen;

internal static class Extensions
{
    extension(TypeDictionary dictionary)
    {
        /// <summary>
        /// Attempts to retrieve the value for key <paramref name="specialType"/> in <paramref name="dictionary"/>,
        /// otherwise throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <param name="specialType"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public INamedTypeSymbol GetOrThrow(SpecialType specialType)
        {
            if (dictionary.TryGetValue(specialType, out INamedTypeSymbol? type))
            {
                return type;
            }

            throw new KeyNotFoundException($"Failed to find special type '{specialType}' in type dictionary.");
        }

        /// <summary>
        /// Attempts to retrieve the value for key <paramref name="specialType"/> in <paramref name="dictionary"/>,
        /// and determine the fully qualified name of the type, otherwise throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <param name="specialType"></param>
        /// <returns></returns>
        public string GetNameOrThrow(SpecialType specialType)
        {
            return dictionary.GetOrThrow(specialType).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }

    extension(ISymbol symbol)
    {
        /// <summary>
        /// Returns the fully qualified name of <paramref name="symbol"/>.
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }
}
