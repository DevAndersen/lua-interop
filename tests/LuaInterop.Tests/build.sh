#!/bin/sh

if ! command -v lua > /dev/null 2>&1
then
    echo "Error: Lua not found"
    exit 1
fi

echo "Building native library to '$1'"
dotnet publish ../LuaInterop.Tests.Demo -r linux-x64 -c Release -o $1
