using System.Diagnostics;
using Xunit;

namespace LuaInterop.Tests;

public static class LuaHelper
{
    public static async Task<string> RunScriptAsync(string script, int timeoutInSeconds = 3)
    {
        ProcessResult processResult = await RunLuaScriptResultAsync(script, timeoutInSeconds);

        Console.WriteLine(processResult.StandardOutput);

        if (!processResult.IsSuccessful)
        {
            Assert.Fail($"Lua script exited with code {processResult.ExitCode}: {processResult.StandardError}");
        }

        return processResult.StandardOutput;
    }

    public static async Task<ProcessResult> RunLuaScriptResultAsync(string script, int timeoutInSeconds = 3)
    {
        string fullScript = $"""
            local interop = require("luainteropdemo")
            {script}
            """;

        string escapedScript = fullScript.Replace("\"", "\\\"");

        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = "lua",
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
