using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UseSymbolAlias
// ReSharper disable StringLiteralTypo

namespace LuaInterop.Native;

public static unsafe partial class Lua
{
    private const string Library = "lua";

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
}
