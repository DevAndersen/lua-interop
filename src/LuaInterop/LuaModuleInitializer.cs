using System.Runtime.InteropServices;
using LuaInterop.Native;

namespace LuaInterop;

public static class LuaModuleInitializer
{
    public static void Initialize()
    {
        // Resolve liblua library file, its name changes between different Linux distros.
        NativeLibrary.SetDllImportResolver(typeof(LuaModuleInitializer).Assembly, (name, assembly, path) =>
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

            throw new Exception("Failed to resolve Lua library file");
        });
    }
}
