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
}
