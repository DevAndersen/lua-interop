using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UseSymbolAlias
// ReSharper disable StringLiteralTypo

namespace LuaInterop.Native;

internal static unsafe partial class Lua
{
    [LibraryImport(Library, EntryPoint = "luaL_checklstring", StringMarshalling = StringMarshalling.Utf8)]
    public static partial byte* CheckLString(
        lua_State L,
        int arg,
        out size_t len);

    [LibraryImport(Library, EntryPoint = "luaL_checkstring", StringMarshalling = StringMarshalling.Utf8)]
    public static partial byte* CheckString(
        lua_State  L,
        int arg);

    [LibraryImport(Library, EntryPoint = "luaL_checkinteger")]
    public static partial lua_Integer CheckInteger(
        lua_State L,
        int arg);

    [LibraryImport(Library, EntryPoint = "luaL_checknumber")]
    public static partial lua_Number CheckNumber(
        lua_State L,
        int arg);
}
