#!/bin/pwsh
docker compose -f "$PSScriptRoot\docker-compose.yml" build

# Lua 5.5

Write-Host -ForegroundColor Green "=== Lua 5.5, Alpine ==="
docker run --rm -it luainterop/distrotesting:55-alpine

Write-Host -ForegroundColor Green "=== Lua 5.5, Arch Linux ==="
docker run --rm -it luainterop/distrotesting:55-archlinux

Write-Host -ForegroundColor Green "=== Lua 5.5, Ubuntu ==="
docker run --rm -it luainterop/distrotesting:55-ubuntu

# Lua 5.4

Write-Host -ForegroundColor Green "=== Lua 5.4, Alpine ==="
docker run --rm -it luainterop/distrotesting:54-alpine

Write-Host -ForegroundColor Green "=== Lua 5.4, Arch Linux ==="
docker run --rm -it luainterop/distrotesting:54-archlinux

Write-Host -ForegroundColor Green "=== Lua 5.4, Ubuntu ==="
docker run --rm -it luainterop/distrotesting:54-ubuntu
