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
        #if !WINDOWS
        ResolveLiblua();
        #endif
    }

    /// <summary>
    /// Attempt to resolve the <c>liblua</c> library file, as the file name varies between Linux distros.
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static void ResolveLiblua()
    {
        NativeLibrary.SetDllImportResolver(typeof(LuaModuleInitializer).Assembly, (name, _, _) =>
        {
            if (name != Lua.Library)
            {
                return nint.Zero;
            }

            string[] potentialLibraryNames =
            [
                "liblua.so", // Arch
                "liblua.so.0",
                "liblua5.5.so", // Ubuntu
                "liblua5.5.so.0",
                "liblua-5.5.so",
                "liblua-5.5.so.0", // Alpine
            ];

            foreach (string str in potentialLibraryNames)
            {
                if (NativeLibrary.TryLoad(str, out nint handle))
                {
                    return handle;
                }
            }

            throw new Exception("Failed to resolve Lua library file"); // Todo: Use a more relevant exception type.
        });
    }
}
