namespace LuaInterop.SourceGen;

internal static class GeneratorConstants
{
    // CoreLib
    public const string UnmanagedCallersOnlyAttributeGlobalFullName = "global::System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute";
    public const string UnmanagedCallersOnlyAttributeFullName = "System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute";
    public const string GeneratedCodeAttributeAttributeGlobalFullName = "global::System.CodeDom.Compiler.GeneratedCodeAttribute";
    public const string TypeMetadataNameDictionary2 = "System.Collections.Generic.IDictionary`2";
    public const string UnmanagedCallersOnlyAttributeEntryPointArgument = "EntryPoint";

    // LuaInterop
    public const string LuaLibraryAttributeFullName = "LuaInterop.Attributes.LuaLibraryAttribute";
    public const string LuaFunctionAttributeFullName = "LuaInterop.Attributes.LuaFunctionAttribute";
    public const string LuaReadHelperTypeGlobalFullName = "global::LuaInterop.LuaReadHelper";
    public const string LuaPushHelperTypeGlobalFullName = "global::LuaInterop.LuaPushHelper";
    public const string LuaInteropHelperTypeGlobalFullName = "global::LuaInterop.LuaInteropHelper";
    public const string LuaInteropHelperRegisterFunctionMethodName = "RegisterFunction";
    public const string LuaFunctionAttributeNameArgumentName = "FunctionName";
    public const string LuaFunctionAttributeManualArgumentName = "ManualFunction";

    // Generator
    public const string GeneratedCodeNamespace = "LuaInterop.Generated";
    public const string LuaInteropHelperCreateTableMethodName = "CreateTable";
    public const string LuaOpenClassName = "LuaEntryPoint";
    public const string LuaOpenMethodName = "LuaOpen";
    public const string LuaStateVariableName = "luaState";
    public const string ReturnVariableName = "returnedValue";
}
