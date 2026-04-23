# FractalKernel

Fractal the fractal. Free for the commons.

A tiny .NET console app that renders a Mandelbrot-inside-Mandelbrot and pins all CPU cores to ~100% while it runs. Built in Dave's box, released to everyone.

## What it does
- Layer 1: classic Mandelbrot
- Layer 2: uses the escape-time to seed a mini-Mandelbrot at each pixel
- Writes a PPM image to your Desktop (`fractal_fractal.ppm`)

## Run
```powershell
dotnet run -c Release
```

Edit `maxIter`, `width`, `height` in Program.cs to make it hotter.

## License
MIT - do whatever you want. No warranty. Watch your temps.
