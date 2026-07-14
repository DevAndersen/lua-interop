using System.Runtime.InteropServices;
using LuaInterop.Native;

namespace LuaInterop;

public static class LuaModuleInitializer
{
    /// <summary>
    /// Performs initialization logic when the library gets called from unmanaged code.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static void Initialize()
    {
        ResolveLuaLibrary();
    }

    /// <summary>
    /// Attempt to resolve and load the installed Lua system library.
    /// </summary>
    /// <exception cref="Exception"></exception>
    internal static void ResolveLuaLibrary()
    {
        NativeLibrary.SetDllImportResolver(typeof(LuaModuleInitializer).Assembly, (name, _, _) =>
        {
            if (name != Lua.LuaLibrary)
            {
                return nint.Zero;
            }

            string[] potentialLibraryNames =
            [
#if WINDOWS && LUA_5_5
                "lua55.dll",
#elif WINDOWS && LUA_5_4
                "lua54.dll",
#elif !WINDOWS && LUA_5_5
                "liblua5.5.so", // Arch, Ubuntu
                "liblua5.5.so.0",
                "liblua-5.5.so",
                "liblua-5.5.so.0", // Alpine
#elif !WINDOWS && LUA_5_4
                "liblua5.4.so", // Arch, Ubuntu
                "liblua5.4.so.0",
                "liblua-5.4.so",
                "liblua-5.4.so.0", // Alpine
#endif
            ];

            foreach (string potentialLibraryName in potentialLibraryNames)
            {
                if (NativeLibrary.TryLoad(potentialLibraryName, out nint handle))
                {
                    return handle;
                }
            }

            throw new Exception("Failed to resolve Lua library file"); // Todo: Use a more relevant exception type.
        });
    }
}
