local interop = require("luainterop")
print(interop.sayMessage("Hello, World!"))

print(type(interop.sayMessage("Hello, World!")))