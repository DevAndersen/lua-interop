#!/bin/sh
dotnet publish ./src/LuaInterop -r linux-x64 -c Release -o . && lua ./LuaInterop.lua
