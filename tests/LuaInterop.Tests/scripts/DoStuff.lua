-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.sayMessage("Hello, World!")

-- Assert
assert(type(result) == "string")
