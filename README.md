# FractalKernel v0.42

**Full 42 body kernel. Build all 40. Dipole.**

This is the commons release.

## What's new
- 42-body parallelism (`MaxDegreeOfParallelism = 42`)
- Two modes:
  - `fractal` — Mandelbrot-inside-Mandelbrot (original)
  - `dipole` — magnetic dipole field fractalization
- Multi-target build: net8.0, net9.0, net10.0, net11.0
- Ready for 40+ RID builds via `dotnet publish -r`

## Run
```powershell
dotnet run -c Release -- fractal
dotnet run -c Release -- dipole
```

Output goes to Desktop as `fractal_fractal_42.ppm` or `fractal_dipole_42.ppm`.

## Build all
```powershell
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-arm64 --self-contained
# ... add more RIDs for the full 40
```

## License
MIT - free for the commons. Push your box to 100% responsibly.
