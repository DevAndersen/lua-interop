using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xunit;

namespace LuaInterop.Tests;

public static class LuaHelper
{
    public static async Task<string> RunScriptAsync([CallerMemberName] string scriptFile = "", int timeoutInSeconds = 3)
    {
        (int exitCode, string standardOutput, string standardError) = await RunLuaScriptAsync(scriptFile, timeoutInSeconds);

        Console.WriteLine(standardOutput);

        if (exitCode != 0 || standardError.Length > 0)
        {
            Assert.Fail($"Lua exited with code {exitCode}: {standardError}");
        }

        return standardOutput;
    }
    
    public static async Task<ProcessResult> RunScriptResultAsync([CallerMemberName] string scriptFile = "", int timeoutInSeconds = 3)
    {
        return await RunLuaScriptAsync(scriptFile, timeoutInSeconds);
    }

    private static async Task<ProcessResult> RunLuaScriptAsync(string scriptFile, int timeoutInSeconds)
    {
        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = "lua",
            Arguments = $"../../../scripts/{scriptFile}.lua",
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
        public bool IsSuccessful => ExitCode == 0 && StandardError.Length > 0;
    }
}
