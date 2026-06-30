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
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodNotAccessible = new DiagnosticDescriptor(
        id: "LUA0002",
        title: "Lua function must be either 'public' or 'internal'",
        messageFormat: "Lua function must be either 'public' or 'internal'",
        category: _category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
