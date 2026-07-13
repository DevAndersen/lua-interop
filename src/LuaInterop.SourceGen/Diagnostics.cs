namespace LuaInterop.SourceGen;

internal static class Diagnostics
{
    private const string _category = "LUA";

    public static readonly DiagnosticDescriptor Debug = new DiagnosticDescriptor(
        id: "LUA9999",
        title: "Debug",
        messageFormat: "Debug diagnostic",
        category: _category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodNotStatic = new DiagnosticDescriptor(
        id: "LUA0001",
        title: "Lua function must be static",
        messageFormat: "Lua function must be static",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodNotAccessible = new DiagnosticDescriptor(
        id: "LUA0002",
        title: "Lua function must be either 'public' or 'internal'",
        messageFormat: "Lua function must be either 'public' or 'internal'",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ReturnTypeNotSupported = new DiagnosticDescriptor(
        id: "LUA0003",
        title: "Unsupported Lua function return type",
        messageFormat: "Unsupported Lua function return type '{0}'",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ParameterTypeNotSupported = new DiagnosticDescriptor(
        id: "LUA0004",
        title: "Unsupported Lua function parameter type",
        messageFormat: "Unsupported Lua function parameter type '{0}'",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ContainingTypeNotAccessible = new DiagnosticDescriptor(
        id: "LUA0005",
        title: "Lua function contained in type not marked as either 'public' or 'internal'",
        messageFormat: "Lua function contained in type {0} which is not marked as either 'public' or 'internal'",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ManualMethodNotReturnInt = new DiagnosticDescriptor(
        id: "LUA0006",
        title: "Manual Lua function must return int",
        messageFormat: "Manual Lua function must return int",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ManualMethodNotAcceptIntPtr = new DiagnosticDescriptor(
        id: "LUA0007",
        title: "Manual Lua function parameters must consist of a single IntPtr parameter",
        messageFormat: "Manual Lua function parameters must consist of a single IntPtr parameter",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ManualMethodMissingUnmanagedCallersOnlyAttribute = new DiagnosticDescriptor(
        id: "LUA0008",
        title: "Manual Lua function must be decorated with UnmanagedCallersOnlyAttribute",
        messageFormat: "Manual Lua function must be decorated with UnmanagedCallersOnlyAttribute",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EmptyFunctionName = new DiagnosticDescriptor(
        id: "LUA0009",
        title: "Custom Lua function name must be a non-empty string",
        messageFormat: "Custom Lua function name must be a non-empty string",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FunctionMarkedAsAsync = new DiagnosticDescriptor(
        id: "LUA0010",
        title: "Lua function must not be marked as async",
        messageFormat: "Lua function must not be marked as async",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FunctionMarkedAsRef = new DiagnosticDescriptor(
        id: "LUA0011",
        title: "Lua function parameter must not be ref-like",
        messageFormat: "Lua function must not be {0}",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FunctionNameNotUnique = new DiagnosticDescriptor(
        id: "LUA0012",
        title: "Lua function name is not unique",
        messageFormat: "Lua function name '{0}' is not unique",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidAssemblyName = new DiagnosticDescriptor(
        id: "LUA0013",
        title: "Assembly name must be a valid identifier and must not contain any dots",
        messageFormat: "Assembly name '{0}' must be a valid identifier and must not contain any dots",
        category: _category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
