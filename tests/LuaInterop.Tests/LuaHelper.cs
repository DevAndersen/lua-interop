using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xunit;

namespace LuaInterop.Tests;

public static class LuaHelper
{
    public static async Task<string> RunScriptAsync([CallerMemberName] string scriptFile = "", int timeoutInSeconds = 3)
    {
        ProcessResult processResult = await RunLuaScriptAsync(scriptFile, timeoutInSeconds);

        Console.WriteLine(processResult.StandardOutput);

        if (!processResult.IsSuccessful)
        {
            Assert.Fail($"Lua script exited with code {processResult.ExitCode}: {processResult.StandardError}");
        }

        return processResult.StandardOutput;
    }

    public static async Task<ProcessResult> RunScriptResultAsync([CallerMemberName] string scriptFile = "", int timeoutInSeconds = 3)
    {
        return await RunLuaScriptAsync(scriptFile, timeoutInSeconds);
    }

    public static async Task<ProcessResult> RunLuaInlineScriptAsync(string script, int timeoutInSeconds = 3)
    {
        string escapedScript = script.Replace("\"", "\\\"");

        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = "lua",
            Arguments = $"-e \"{escapedScript}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = "../../../scripts" // Path of scripts directory, relative to the test executable.
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

    private static async Task<ProcessResult> RunLuaScriptAsync(string scriptFile, int timeoutInSeconds)
    {
        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = "lua",
            Arguments = $"{scriptFile}.lua",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = "../../../scripts" // Path of scripts directory, relative to the test executable.
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
