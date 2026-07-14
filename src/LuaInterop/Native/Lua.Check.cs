// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UseSymbolAlias
// ReSharper disable StringLiteralTypo

namespace LuaInterop.Native;

internal static unsafe partial class Lua
{
    [LibraryImport(LuaLibrary, EntryPoint = "luaL_checklstring", StringMarshalling = StringMarshalling.Utf8)]
    public static partial byte* CheckLString(
        lua_State L,
        int arg,
        out size_t len);

    [LibraryImport(LuaLibrary, EntryPoint = "luaL_checkstring", StringMarshalling = StringMarshalling.Utf8)]
    public static partial byte* CheckString(
        lua_State L,
        int arg);

    [LibraryImport(LuaLibrary, EntryPoint = "luaL_checkinteger")]
    public static partial lua_Integer CheckInteger(
        lua_State L,
        int arg);

    [LibraryImport(LuaLibrary, EntryPoint = "luaL_checknumber")]
    public static partial lua_Number CheckNumber(
        lua_State L,
        int arg);

    [LibraryImport(LuaLibrary, EntryPoint = "lua_toboolean")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ToBoolean(
        lua_State L,
        int index);

    [LibraryImport(LuaLibrary, EntryPoint = "lua_type")]
    public static partial LuaType Type(
        lua_State L,
        int index);

    [LibraryImport(LuaLibrary, EntryPoint = "lua_gettop")]
    public static partial int GetTop(
        lua_State L);
}
