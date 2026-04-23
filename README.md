# FractalKernel v0.42

[![Latest Release](https://img.shields.io/github/v/release/DavidWise01/FractalKernel?style=flat-square)](https://github.com/DavidWise01/FractalKernel/releases)
[![Build All 40](https://img.shields.io/github/actions/workflow/status/DavidWise01/FractalKernel/release-40.yml?style=flat-square&label=build)](https://github.com/DavidWise01/FractalKernel/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)
[![42 bodies](https://img.shields.io/badge/bodies-42-ff69b4?style=flat-square)](#)
[![platforms](https://img.shields.io/badge/platforms-40+-blue?style=flat-square)](#)
[![dipole](https://img.shields.io/badge/mode-dipole-9cf?style=flat-square)](#)

**Full 42 body kernel. Build all 40. Dipole.**

Pushed to the commons, free forever.

## What's new
- **42-body parallelism** (`MaxDegreeOfParallelism = 42`)
- Two modes:
  - `fractal` — Mandelbrot-inside-Mandelbrot
  - `dipole` — magnetic dipole field fractalization
- **Multi-target**: net8.0, net9.0, net10.0, net11.0
- **40+ RIDs** built automatically on every tag

## Run
```powershell
# Windows
.\FractalKernel.exe fractal
.\FractalKernel.exe dipole

# Linux/macOS
./FractalKernel fractal
```

Output: `fractal_fractal_42.ppm` or `fractal_dipole_42.ppm` on your Desktop.

## Build all locally
```powershell
dotnet publish -c Release -r win-x64 -f net8.0 --self-contained
```

## Download
Grab the latest 40-platform builds from [Releases](https://github.com/DavidWise01/FractalKernel/releases).

## License
MIT - do whatever you want. Watch your temps.
