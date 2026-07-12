#!/bin/pwsh
docker compose build

Write-Host -ForegroundColor Green "=== Alpine ==="
docker run --rm -it luainterop/distrotesting:alpine

Write-Host -ForegroundColor Green "=== Arch Linux ==="
docker run --rm -it luainterop/distrotesting:archlinux

Write-Host -ForegroundColor Green "=== Ubuntu 26.04 ==="
docker run --rm -it luainterop/distrotesting:ubuntu2604
