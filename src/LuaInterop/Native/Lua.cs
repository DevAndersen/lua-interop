using System.Runtime.InteropServices;

namespace LuaInterop.Native;

internal static partial class Lua
{
    private const string Library = "lua";

    [LibraryImport(Library)]
    internal static partial void lua_createtable(
        nint L,
        int narr,
        int nrec);

    [LibraryImport(Library)]
    internal static partial void lua_pushcclosure(
        nint L,
        nint fn,
        int n);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void lua_setfield(
        nint L,
        int idx,
        string k);

    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint lua_pushstring(
        nint L,
        string s);

    [LibraryImport(Library)]
    internal static partial nint luaL_checklstring(
        nint L,
        int arg,
        out nuint len);
}
