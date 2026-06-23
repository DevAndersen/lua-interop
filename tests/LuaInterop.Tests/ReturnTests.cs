using Xunit;

namespace LuaInterop.Tests;

public class ReturnTests
{
    [Fact]
    public async Task ReturnString_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync("""
            -- Act
            local result = interop.returnString()

            -- Assert
            assert(type(result) == "string")
            assert(result == "Hello, World!")
            """);
    }

    [Fact]
    public async Task ReturnBoolean_False_ReturnsFalse()
    {
        await LuaHelper.RunScriptAsync("""
            -- Act
            local result = interop.returnBooleanFalse()

            -- Assert
            assert(type(result) == "boolean")
            assert(result == false)
            """);
    }

    [Fact]
    public async Task ReturnBoolean_True_ReturnsTrue()
    {
        await LuaHelper.RunScriptAsync("""
            -- Act
            local result = interop.returnBooleanTrue()

            -- Assert
            assert(type(result) == "boolean")
            assert(result == true)
            """);
    }

    [Fact]
    public async Task ReturnInteger_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync("""
            -- Act
            local result = interop.returnInteger()

            -- Assert
            assert(math.type(result) == "integer")
            assert(result == 42)
            """);
    }

    [Fact]
    public async Task ReturnNumber_ExpectedValue()
    {
        await LuaHelper.RunScriptAsync("""
            -- Act
            local result = interop.returnNumber()

            -- Assert
            assert(type(result) == "number")
            assert(result == 123.456)
            """);
    }

    [Fact]
    public async Task ReturnNull_ExpectedNull()
    {
        await LuaHelper.RunScriptAsync("""
            -- Act
            local result = interop.returnNull()

            -- Assert
            assert(type(result) == "nil")
            assert(result == nil)
            """);
    }
}
