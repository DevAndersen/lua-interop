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
        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, (name, assembly, path) =>
        {
            if (name == Lua.Library)
            {
                if (NativeLibrary.TryLoad(Lua.Library, out nint handle))
                {
                    return handle;
                }
                else if (NativeLibrary.TryLoad("liblua5.5.so", out handle))
                {
                    return handle;
                }
                throw new Exception($"Failed to resolve Lua library file");
            }

            return nint.Zero;
        });
    }
}
#endif

// Todo: Call this from source generator, move [ModuleInitializer] declaration over to source generator.
// Todo: Consolidate the two Unix filename paths in the same file.
