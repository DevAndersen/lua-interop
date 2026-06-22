-- Arrange
local interop = require("luainteropdemo")

-- Act
local result = interop.returnNumber()

-- Assert
assert(type(result) == "number")
assert(result == 123.456)
