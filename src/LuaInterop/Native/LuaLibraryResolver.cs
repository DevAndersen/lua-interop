#if !WINDOWS
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LuaInterop.Native;

internal static class NativeLibraryResolver
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void ModuleInit()
    {
        // Resolve liblua library file, its name changes between different Linux distros.
        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, (name, assembly, path) =>
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
#endif

// Todo: Call this from source generator, move [ModuleInitializer] declaration over to source generator.
// Todo: Consolidate the two Unix filename paths in the same file.
