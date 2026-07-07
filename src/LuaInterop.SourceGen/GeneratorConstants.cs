namespace LuaInterop.SourceGen;

internal static class GeneratorConstants
{
    public const string GeneratedCodeNamespace = "LuaInterop.Generated";
    public const string LuaLibraryAttributeFullName = "LuaInterop.Attributes.LuaLibraryAttribute";
    public const string LuaFunctionAttributeFullName = "LuaInterop.Attributes.LuaFunctionAttribute";
    public const string LuaFunctionAttributeNameArgumentName = "FunctionName";
    public const string LuaInteropHelperTypeFullName = "global::LuaInterop.LuaInteropHelper";
    public const string LuaInteropHelperRegisterFunctionMethodName = "RegisterFunction";
    public const string LuaInteropHelperCreateTableMethodName = "CreateTable";
    public const string LuaReadHelperTypeFullName = "global::LuaInterop.LuaReadHelper";
    public const string LuaPushHelperTypeFullName = "global::LuaInterop.LuaPushHelper";
    public const string UnmanagedCallersOnlyAttributeFullName = "global::System.Runtime.InteropServices.UnmanagedCallersOnly";
    public const string GeneratedCodeAttributeAttributeFullName = "global::System.CodeDom.Compiler.GeneratedCodeAttribute";
    public const string UnmanagedCallersOnlyAttributeEntryPointArgument = "EntryPoint";
    public const string ReturnVariableName = "returnedValue";
    public const string LuaOpenClassName = "LuaEntryPoint";
    public const string LuaOpenMethodName = "LuaOpen";
    public const string LuaStateVariableName = "luaState";

    public const string TypeMetadataNameDictionary2 = "System.Collections.Generic.IDictionary`2";

}
