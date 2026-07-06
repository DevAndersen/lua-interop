using System.Runtime.InteropServices;

namespace LuaInterop.Native;

internal static partial class Lua
{
    [LibraryImport(Library, EntryPoint = "lua_pushvalue")]
    public static partial void PushValue(
        lua_State L,
        int idx);

    [LibraryImport(Library, EntryPoint = "lua_pushcclosure")]
    public static partial void PushCClosure(
        lua_State L,
        nint fn,
        int n);

    [LibraryImport(Library, EntryPoint = "lua_pushlstring", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint PushLString(
        lua_State L,
        string s,
        size_t len);

    [LibraryImport(Library, EntryPoint = "lua_pushboolean")]
    public static partial void PushBoolean(
        lua_State L,
        [MarshalAs(UnmanagedType.Bool)] bool b);

    [LibraryImport(Library, EntryPoint = "lua_pushinteger")]
    public static partial void PushInteger(
        lua_State L,
        lua_Integer n);

    [LibraryImport(Library, EntryPoint = "lua_pushnumber")]
    public static partial void PushNumber(
        lua_State L,
        lua_Number n);

    [LibraryImport(Library, EntryPoint = "lua_pushnil")]
    public static partial void PushNil(
        lua_State L);

    [LibraryImport(Library, EntryPoint = "lua_rawset")]
    public static partial void RawSet(
        lua_State L,
        int index);
}
