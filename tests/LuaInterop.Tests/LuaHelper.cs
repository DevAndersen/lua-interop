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
            local interop = require("luainteropdemo")
            {script}
            """;

        string escapedScript = fullScript.Replace("\"", "\\\"");

        string luaExecutableFileName;
        if (OperatingSystem.IsWindows())
        {
            luaExecutableFileName = "lua";
        }
        else
        {
            // Todo: Note this down as needing to be changed if targeting a different version of Lua.
            luaExecutableFileName = "lua5.5";
        }

        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = luaExecutableFileName,
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
}
