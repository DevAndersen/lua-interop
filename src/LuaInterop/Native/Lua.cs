// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UseSymbolAlias
// ReSharper disable StringLiteralTypo

namespace LuaInterop.Native;

internal static partial class Lua
{
    /// <remarks>
    /// This value is a placeholder, which gets resolved by <see cref="LuaModuleInitializer"/>.
    /// </remarks>
    internal const string LuaLibrary = nameof(LuaLibrary);

    [LibraryImport(LuaLibrary, EntryPoint = "lua_createtable")]
    public static partial void CreateTable(
        lua_State L,
        int nseq,
        int nrec);

    [LibraryImport(LuaLibrary, EntryPoint = "lua_setfield", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SetField(
        lua_State L,
        int index,
        string k);

    [LibraryImport(LuaLibrary, EntryPoint = "lua_pcallk")]
    public static partial LuaStatusCode PCallK(
        lua_State L,
        int nargs,
        int nresults,
        int msgh,
        nint ctx,
        nint k);
}
