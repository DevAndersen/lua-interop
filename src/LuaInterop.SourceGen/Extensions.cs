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

            throw new Exception($"Failed to find special type '{specialType}' in type dictionary."); // Todo: Find an appropriate exception type.
        }

        public string GetNameOrThrow(SpecialType specialType)
        {
            return dictionary.GetOrThrow(specialType).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }
}
