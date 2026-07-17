namespace LuaInterop.SourceGen;

internal static class GeneratorConstants
{
    // CoreLib
    public const string UnmanagedCallersOnlyAttributeGlobalFullName = "global::System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute";
    public const string UnmanagedCallersOnlyAttributeFullName = "System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute";
    public const string GeneratedCodeAttributeAttributeGlobalFullName = "global::System.CodeDom.Compiler.GeneratedCodeAttribute";
    public const string ModuleInitializerAttributeAttributeGlobalFullName = "global::System.Runtime.CompilerServices.ModuleInitializerAttribute";
    public const string TypeMetadataNameDictionary2 = "System.Collections.Generic.IDictionary`2";
    public const string UnmanagedCallersOnlyAttributeEntryPointArgument = "EntryPoint";

    // LuaInterop
    public const string LuaLibraryAttributeFullName = "LuaInterop.Attributes.LuaLibraryAttribute";
    public const string LuaLibraryAttributeInitializerArgumentName = "GenerateModuleInitializerClass";
    public const string LuaFunctionAttributeFullName = "LuaInterop.Attributes.LuaFunctionAttribute";
    public const string LuaReadHelperTypeGlobalFullName = "global::LuaInterop.LuaReadHelper";
    public const string LuaPushHelperTypeGlobalFullName = "global::LuaInterop.LuaPushHelper";
    public const string LuaInteropHelperTypeGlobalFullName = "global::LuaInterop.LuaInteropHelper";
    public const string LuaInteropHelperRegisterFunctionMethodName = "RegisterFunction";
    public const string LuaFunctionAttributeNameArgumentName = "FunctionName";
    public const string LuaFunctionAttributeManualArgumentName = "ManualFunction";
    public const string LuaReadHelperParameterCountMethodName = "ThrowIfUnexpectedParameterCount";

    // Generator
    public const string GeneratedCodeNamespace = "LuaInterop.Generated";
    public const string LuaInteropHelperCreateTableMethodName = "CreateTable";
    public const string LuaOpenClassName = "LuaEntryPoint";
    public const string LuaOpenMethodName = "LuaOpen";
    public const string LuaStateVariableName = "luaState";
    public const string ReturnVariableName = "returnedValue";

    // Module initializer
    public const string ModuleInitializerClassName = "LuaInteropModuleInitializer";
    public const string ModuleInitializer = "InitializeModule";
    public const string ModuleInitializerHelperTypeGlobalFullName = "global::LuaInterop.LuaModuleInitializer";
    public const string ModuleInitializerHelperMethodName = "Initialize";
}
