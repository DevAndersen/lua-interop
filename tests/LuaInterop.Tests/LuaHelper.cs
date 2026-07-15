using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LuaInterop.Tests;

public static class LuaHelper
{
    private const string _luaSyntaxName = "Lua";

    public static async Task<string> RunScriptAsync([StringSyntax(_luaSyntaxName)] string script, int timeoutInSeconds = 3)
    {
        ProcessResult processResult = await RunLuaScriptResultAsync(script, timeoutInSeconds);

        Console.WriteLine(processResult.StandardOutput);

        if (!processResult.IsSuccessful)
        {
            Assert.Fail($"Lua script exited with code {processResult.ExitCode}: {processResult.StandardError}");
        }

        return processResult.StandardOutput;
    }

    public static async Task<ProcessResult> RunLuaScriptResultAsync([StringSyntax(_luaSyntaxName)] string script, int timeoutInSeconds = 3)
    {
        // language=Lua
        string fullScript = $"""
            package.cpath = "./nativelib/?.dll;./nativelib/?.so;" .. package.cpath
            local interop = require("LuaInterop_Tests_Demo")
            {script}
            """;

        string escapedScript = fullScript.Replace("\"", "\\\"");

        string luaExecutableFileName = GetLuaExecutableName();

        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = luaExecutableFileName ?? throw new Exception("Lua executable not defined"),
            Arguments = $"-e \"{escapedScript}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        if (process == null)
        {
            Assert.Fail("Lua process was null");
        }

        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));

        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Assert.Fail("Lua script timed out");
        }

        string standardOutput = await process.StandardOutput.ReadToEndAsync(cts.Token);
        string standardError = await process.StandardError.ReadToEndAsync(cts.Token);
        int exitCode = process.ExitCode;

        return new ProcessResult(exitCode, standardOutput, standardError);
    }

    public record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public bool IsSuccessful => ExitCode == 0 && StandardError.Length == 0;
    }

    /// <summary>
    /// Returns the name of the Lua executable.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="PlatformNotSupportedException"></exception>
    private static string GetLuaExecutableName()
    {
        if (OperatingSystem.IsWindows())
        {
#if LUA_5_5
            return "lua55";
#elif LUA_5_4
            return "lua54";
#else
            throw new NotSupportedException("Lua executable name not defined for the specified version of Lua");
#endif
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
#if LUA_5_5
            return "lua5.5";
#elif LUA_5_4
            return "lua5.4";
#else
            throw new NotSupportedException("Lua executable name not defined for the specified version of Lua");
#endif
        }
        else
        {
            throw new PlatformNotSupportedException("Operating system not supported");
        }
    }
}
