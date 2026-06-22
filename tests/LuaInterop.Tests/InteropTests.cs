using Xunit;

namespace LuaInterop.Tests;

public class InteropTests
{
    [Fact]
    public async Task ReturnString_ReturnsExpectedString()
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
}
