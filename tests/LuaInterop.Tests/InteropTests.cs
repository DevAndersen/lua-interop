using Xunit;

namespace LuaInterop.Tests;

public class InteropTests
{
    [Fact]
    public async Task DoStuff()
    {
        await LuaHelper.RunScriptAsync();
    }
}
