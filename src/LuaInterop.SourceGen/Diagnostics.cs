using Microsoft.CodeAnalysis;

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
}
