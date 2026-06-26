#!/bin/sh

echo "Building native library to '$1'"
dotnet publish ../LuaInterop.Tests.Demo -r linux-x64 -c Release -o $1
