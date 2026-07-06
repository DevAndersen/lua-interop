using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UseSymbolAlias
// ReSharper disable StringLiteralTypo

namespace LuaInterop.Native;

internal static partial class Lua
{
#if WINDOWS
    private const string Library = "lua55.dll";
#else
    private const string Library = "lua";
#endif

    [LibraryImport(Library, EntryPoint = "lua_createtable")]
    public static partial void CreateTable(
        lua_State L,
        int narr,
        int nrec);

    [LibraryImport(Library, EntryPoint = "lua_setfield", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SetField(
        lua_State L,
        int idx,
        string k);

    [LibraryImport(Library, EntryPoint = "lua_pcallk")]
    public static partial LuaStatusCode PCallK(
        lua_State L,
        int nargs,
        int nresults,
        int msgh,
        nint ctx,
        nint k);
}
