namespace LuaInterop.SourceGen;

/// <summary>
/// Dictionary keys for use with <see cref="TypeDictionary"/>.
/// </summary>
internal enum TypeDictionaryId
{
    /// <summary>
    /// <see cref="int"/>.
    /// </summary>
    Int,

    /// <summary>
    /// <see cref="System.IntPtr"/>.
    /// </summary>
    IntPtr,

    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    Dictionary2,

    /// <summary>
    /// <c>LuaLibraryAttribute</c>.
    /// </summary>
    LuaLibraryAttribute,

    /// <summary>
    /// <c>LuaFunctionAttribute</c>.
    /// </summary>
    LuaFunctionAttribute,

    /// <summary>
    /// <c>UnmanagedCallersOnlyAttribute</c>.
    /// </summary>
    UnmanagedCallersOnlyAttribute
}
