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

            IEnumerable<string> potentialLibraryNames = GetLuaLibraryNames();

            foreach (string potentialLibraryName in potentialLibraryNames)
            {
                if (NativeLibrary.TryLoad(potentialLibraryName, out nint handle))
                {
                    return handle;
                }
            }

            throw new DllNotFoundException($"Failed to resolve Lua library file, looked for [{string.Join(", ", potentialLibraryNames)}]");
        });
    }

    /// <summary>
    /// Returns potential names for the Lua library file.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="PlatformNotSupportedException"></exception>
    private static IEnumerable<string> GetLuaLibraryNames()
    {
        if (OperatingSystem.IsWindows())
        {
#if LUA_5_5
            yield return "lua55.dll";
#elif LUA_5_4
            yield return "lua54.dll";
#else
            throw new NotSupportedException("Lua library name not defined for the specified version of Lua");
#endif
        }
        else if (OperatingSystem.IsLinux())
        {
#if LUA_5_5
            yield return "liblua5.5.so"; // Arch, Debian
            yield return "liblua5.5.so.0"; // OpenSUSE
            yield return "liblua-5.5.so"; // Fedora
            yield return "liblua-5.5.so.0"; // Alpine
#elif LUA_5_4
            yield return "liblua5.4.so"; // Arch, Debian
            yield return "liblua5.4.so.0"; // OpenSUSE
            yield return "liblua-5.4.so"; // Fedora
            yield return "liblua-5.4.so.0"; // Alpine
#else
            throw new NotSupportedException("Lua library name not defined for the specified version of Lua");
#endif
        }
        else if (OperatingSystem.IsMacOS())
        {
#if LUA_5_5
            yield return "liblua5.5.dylib";
#elif LUA_5_4
            yield return "liblua5.4.dylib";
#else
            throw new NotSupportedException("Lua library name not defined for the specified version of Lua");
#endif
        }
        else
        {
            throw new PlatformNotSupportedException("Operating system not supported");
        }
    }
}
