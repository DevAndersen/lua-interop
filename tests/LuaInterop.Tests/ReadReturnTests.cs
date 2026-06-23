using Xunit;

namespace LuaInterop.Tests;

public class ReadReturnTests
{
    [Fact]
    public async Task ReadReturnString_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync("""
            -- Arrange
            local arg = "Hello, World!"

            -- Act
            local result = interop.readReturnString(arg)

            -- Assert
            assert(type(result) == "string")
            assert(result == arg)
            """);
    }

    [Fact]
    public async Task ReadReturnInteger_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync("""
            -- Arrange
            local arg = 42

            -- Act
            local result = interop.readReturnInteger(arg)

            -- Assert
            assert(math.type(result) == "integer")
            assert(result == arg)
            """);
    }

    [Fact]
    public async Task ReadReturnNumber_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync("""
            -- Arrange
            local arg = 123.456

            -- Act
            local result = interop.readReturnNumber(arg)

            -- Assert
            assert(type(result) == "number")
            assert(result == arg)
            """);
    }

    [Fact]
    public async Task Test()
    {
        await LuaHelper.RunScriptAsync("""
            local arg = 42
            local result = interop.readReturnInteger(arg)

            assert(math.type(result) == "integer")
            assert(result == 42)
            """);
    }
}
