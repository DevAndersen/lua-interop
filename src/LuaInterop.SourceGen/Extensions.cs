namespace LuaInterop.SourceGen;

internal static class Extensions
{
    extension(TypeDictionary dictionary)
    {
        /// <summary>
        /// Attempts to retrieve the value for key <paramref name="typeId"/> in <paramref name="dictionary"/>,
        /// otherwise throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public INamedTypeSymbol GetOrThrow(TypeDictionaryId typeId)
        {
            if (dictionary.TryGetValue(typeId, out INamedTypeSymbol? type))
            {
                return type;
            }

            throw new KeyNotFoundException($"Failed to find special type '{typeId}' in type dictionary.");
        }

        /// <summary>
        /// Attempts to retrieve the value for key <paramref name="typeId"/> in <paramref name="dictionary"/>,
        /// and determine the fully qualified name of the type, otherwise throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public string GetNameOrThrow(TypeDictionaryId typeId)
        {
            return dictionary.GetOrThrow(typeId).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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
