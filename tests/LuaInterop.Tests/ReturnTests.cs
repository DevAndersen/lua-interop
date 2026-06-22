using Xunit;

namespace LuaInterop.Tests;

public class ReturnTests
{
    [Fact]
    public async Task ReturnString_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task ReturnBoolean_False_ReturnsFalse()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task ReturnBoolean_True_ReturnsTrue()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task ReturnInteger_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task ReturnNumber_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync();
    }

    [Fact]
    public async Task ReturnNull_ExpectedNull()
    {
        await LuaHelper.RunScriptAsync();
    }
}
