using System.Runtime.InteropServices;
using lua_Integer = long;
using lua_State = nint;
using size_t = nuint;
using lua_Number = double;
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

    [LibraryImport(Library, EntryPoint = "lua_pushcclosure")]
    public static partial void PushCClosure(
        lua_State L,
        nint fn,
        int n);

    [LibraryImport(Library, EntryPoint = "lua_setfield", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SetField(
        lua_State L,
        int idx,
        string k);

    [LibraryImport(Library, EntryPoint = "lua_pushstring", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint PushString(
        lua_State L,
        string s);

    [LibraryImport(Library, EntryPoint = "lua_pushboolean")]
    public static partial void PushBoolean(
        lua_State L,
        [MarshalAs(UnmanagedType.Bool)] bool b);

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
