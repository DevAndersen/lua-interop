using LuaInterop.Attributes;
using System.Runtime.InteropServices;

namespace LuaInterop.Tests.Demo;

public static class Debugging
{
#if DEBUG
#pragma warning disable IDE0051 // Remove unused private member
    // For navigating to source generated code.
    private const string _generatedClassName = nameof(Generated.LuaEntryPoint_luainteropdemo);
#pragma warning restore IDE0051 // Remove unused private member
#endif

    [LuaFunction(ManualFunction = true)]
    [UnmanagedCallersOnly]
    public static int CallbackTest(nint luaState)
    {
        // Read integer input (argument 1).
        int arg = LuaReadHelper.ReadInt(luaState, 1, "");

        // Push function to top (argument 2).
        LuaInteropHelper.PushValue(luaState, 2);

        // Push function argument.
        int argumentCount = LuaPushHelper.PushInt(luaState, arg * 2);

        // Call function.
        LuaInteropHelper.CallFunction(luaState, argumentCount, 1, "Callbacktest"); // Use the registered function name.

        // Read function return value.
        int result = LuaReadHelper.ReadInt(luaState, -1, "");
        Console.WriteLine($"Callback returned: {result}");

        return 0;
    }
}
