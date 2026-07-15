; Shipped analyzer release

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LUA0001 | LUA | Error | Lua function must be static
LUA0002 | LUA | Error | Lua function must be either 'public' or 'internal'
LUA0003 | LUA | Error | Unsupported Lua function return type
LUA0004 | LUA | Error | Unsupported Lua function parameter type
LUA0005 | LUA | Error | Lua function contained in type not marked as either 'public' or 'internal'
LUA0006 | LUA | Error | Manual Lua function must return int
LUA0007 | LUA | Error | Manual Lua function parameters must consist of a single IntPtr parameter
LUA0008 | LUA | Error | Manual Lua function must be decorated with UnmanagedCallersOnlyAttribute
LUA0009 | LUA | Error | Custom Lua function name must be a non-empty string
LUA0010 | LUA | Error | Lua function must not be marked as async
LUA0011 | LUA | Error | Lua function parameter must not be ref-like
LUA0012 | LUA | Error | Lua function name is not unique
LUA0013 | LUA | Error | Assembly name must be a valid identifier and must not contain any dots
LUA0014 | LUA | Error | Failed to resolve required type
