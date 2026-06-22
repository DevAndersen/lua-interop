using System.Runtime.InteropServices;

namespace LuaInterop.Native;

public static unsafe partial class Lua
{
    [LibraryImport(Library, EntryPoint = "lua_pushcclosure")]
    public static partial void PushCClosure(
        lua_State L,
        nint fn,
        int n);

    [LibraryImport(Library, EntryPoint = "lua_pushstring", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint PushString(
        lua_State L,
        string s);

    [LibraryImport(Library, EntryPoint = "lua_pushboolean")]
    public static partial void PushBoolean(
        lua_State L,
        [MarshalAs(UnmanagedType.Bool)] bool b);
}
