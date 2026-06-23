using Xunit;

namespace LuaInterop.Tests;

public class ReadReturnTests
{
    [Fact]
    public async Task ReadReturnString_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task ReadReturnInteger_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task ReadReturnNumber_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task Test()
    {
        string script = """
            local interop = require("luainteropdemo")
            
            local arg = 42
            local result = interop.readReturnInteger(arg)
            
            assert(math.type(result) == "integer")
            assert(result == 43)
            """;

        LuaHelper.ProcessResult processResult = await LuaHelper.RunLuaInlineScriptAsync(script);


        if (!processResult.IsSuccessful)
        {
            Assert.Fail($"Lua script exited with code {processResult.ExitCode}: {processResult.StandardError}");
        }
    }
}
