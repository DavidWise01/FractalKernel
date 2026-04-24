// FractalKernel v0.42 — 42-body invariant kernel
// 40 hardware bodies (8×{cap,tor,dio,res,bat}) + 1 motherboard + 1 lumen = 42
// Lumen = 16 fractal colors orbiting 1 pure white center (palette index 0 = white)
// Modes: fractal (Mandelbrot-inside-Mandelbrot) | dipole (magnetic dipole field)
// MaxDegreeOfParallelism = 42 (the invariant)
// MIT — pushed to the commons.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FractalKernel;

internal static class Program
{
    // ===== INVARIANTS =====
    private const int W = 1920;
    private const int H = 1080;
    private const int MAX_ITER = 96;
    private const int MAX_DOP = 42;            // 42-body parallelism
    private const string VERSION = "0.42";
    private const string HASH_LOCK = "02880745b847317c4e2424524ec25d0f7a2b84368d184586f45b54af9fcab763";

    // 42-body roster (declarative — for reporting)
    private static readonly (string label, int n)[] Bodies =
    {
        ("cap", 8), ("tor", 8), ("dio", 8), ("res", 8), ("bat", 8),
        ("mb",  1), ("lumen", 1)
    };

    // ===== LUMEN PALETTE: 16 fractal + 1 pure white = 17 =====
    // Index 0 = pure white (deepest convergence)
    // Index 1..16 = 16 hue-spaced colors orbiting the white center
    private static readonly byte[][] Palette = MakePalette();

    private static byte[][] MakePalette()
    {
        var p = new byte[17][];
        p[0] = new byte[] { 255, 255, 255 }; // 17 = pure white center
        for (int i = 0; i < 16; i++)
        {
            double h = i / 16.0 * 360.0;
            double s = 0.72;
            double l = 0.50 + 0.06 * Math.Sin(i * Math.PI / 4);
            p[i + 1] = HslToRgb(h, s, l);
        }
        return p;
    }

    private static byte[] HslToRgb(double h, double s, double l)
    {
        h = (h % 360 + 360) % 360;
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
        double m = l - c / 2;
        double r, g, b;
        if (h < 60)       { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else              { r = c; g = 0; b = x; }
        return new[]
        {
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255)
        };
    }

    // ===== FRACTAL: Mandelbrot-inside-Mandelbrot =====
    // Pass 1: standard escape-time iteration on c.
    // Pass 2: for points that don't escape, terminal z becomes new c, run secondary.
    // Result: escape ring outside, fractal banding inside, white at deep double-convergence.
    private static int FractalIter(double cx, double cy, int maxIter)
    {
        double zx = 0, zy = 0;
        for (int i = 0; i < maxIter; i++)
        {
            double zx2 = zx * zx, zy2 = zy * zy;
            if (zx2 + zy2 > 4) return 1 + i % 16;
            double ny = 2 * zx * zy + cy;
            zx = zx2 - zy2 + cx;
            zy = ny;
        }
        // Inside main set — secondary Mandelbrot using terminal z as seed
        double cx2 = zx, cy2 = zy;
        double wx = 0, wy = 0;
        int hi = maxIter >> 1;
        for (int j = 0; j < hi; j++)
        {
            double wx2 = wx * wx, wy2 = wy * wy;
            if (wx2 + wy2 > 4) return 1 + (j + 8) % 16;
            double nwy = 2 * wx * wy + cy2;
            wx = wx2 - wy2 + cx2;
            wy = nwy;
        }
        return 0; // pure white = deepest double-convergence
    }

    // ===== DIPOLE: magnetic dipole field, fractalized =====
    // Two poles at (-0.7, 0) and (+0.7, 0).
    // Compute dipole potential per pixel, use as complex c-value, iterate z = z² + c.
    // Field topology becomes fractal scaffolding; null line = white.
    private static int DipoleIter(double x, double y, int maxIter)
    {
        double dx1 = x + 0.7, dy1 = y;
        double dx2 = x - 0.7, dy2 = y;
        double r1 = dx1 * dx1 + dy1 * dy1 + 1e-6;
        double r2 = dx2 * dx2 + dy2 * dy2 + 1e-6;
        double cx = (dx1 / r1 - dx2 / r2) * 0.35;
        double cy = (dy1 / r1 - dy2 / r2) * 0.35;
        double zx = 0, zy = 0;
        for (int i = 0; i < maxIter; i++)
        {
            double zx2 = zx * zx, zy2 = zy * zy;
            if (zx2 + zy2 > 4) return 1 + i % 16;
            double ny = 2 * zx * zy + cy;
            zx = zx2 - zy2 + cx;
            zy = ny;
        }
        return 0; // converged: field equilibrium
    }

    // ===== MAIN =====
    public static int Main(string[] args)
    {
        string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "fractal";
        if (mode != "fractal" && mode != "dipole")
        {
            PrintUsage();
            return mode is "-h" or "--help" or "help" ? 0 : 1;
        }

        PrintBanner(mode);

        // View parameters per mode
        double centerX = mode == "fractal" ? -0.5 : 0.0;
        double centerY = 0.0;
        double scale   = mode == "fractal" ? 3.5  : 4.0;
        double sx = scale / W;
        double sy = scale / W; // square pixels
        double x0 = centerX - W * sx / 2;
        double y0 = centerY - H * sy / 2;

        // Buffer: RGB per pixel
        byte[] buffer = new byte[W * H * 3];

        var sw = Stopwatch.StartNew();

        // === 42-body parallel render ===
        var po = new ParallelOptions { MaxDegreeOfParallelism = MAX_DOP };
        Parallel.For(0, H, po, py =>
        {
            double y = y0 + py * sy;
            int rowOffset = py * W * 3;
            if (mode == "fractal")
            {
                for (int px = 0; px < W; px++)
                {
                    double x = x0 + px * sx;
                    int k = FractalIter(x, y, MAX_ITER);
                    var c = Palette[k];
                    int o = rowOffset + px * 3;
                    buffer[o]     = c[0];
                    buffer[o + 1] = c[1];
                    buffer[o + 2] = c[2];
                }
            }
            else
            {
                for (int px = 0; px < W; px++)
                {
                    double x = x0 + px * sx;
                    int k = DipoleIter(x, y, MAX_ITER);
                    var c = Palette[k];
                    int o = rowOffset + px * 3;
                    buffer[o]     = c[0];
                    buffer[o + 1] = c[1];
                    buffer[o + 2] = c[2];
                }
            }
        });

        sw.Stop();

        // === Output ===
        string outPath = ResolveOutputPath(mode);
        WritePpm(outPath, W, H, buffer);

        var fi = new FileInfo(outPath);
        Console.WriteLine($"render: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"output: {outPath}");
        Console.WriteLine($"size:   {fi.Length / 1024} KB");
        Console.WriteLine();
        Console.WriteLine("done. 42 bodies, 1 lumen, 1 motherboard. invariant held.");
        return 0;
    }

    private static string ResolveOutputPath(string mode)
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (string.IsNullOrEmpty(desktop) || !Directory.Exists(desktop))
            desktop = Directory.GetCurrentDirectory();
        return Path.Combine(desktop, $"fractal_{mode}_42.ppm");
    }

    private static void WritePpm(string path, int w, int h, byte[] rgb)
    {
        using var fs = File.Create(path);
        // PPM P6 binary header
        byte[] header = Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n");
        fs.Write(header, 0, header.Length);
        fs.Write(rgb, 0, rgb.Length);
    }

    private static void PrintBanner(string mode)
    {
        Console.WriteLine($"FractalKernel v{VERSION} — mode: {mode}");
        Console.WriteLine($"42-body invariant: {DescribeBodies()}");
        Console.WriteLine($"MaxDoP = {MAX_DOP}");
        Console.WriteLine($"resolution: {W}×{H}");
        Console.WriteLine($"lumen palette: 16 fractal + 1 pure white = 17");
        Console.WriteLine($"hash: {HASH_LOCK[..16]}…");
        Console.WriteLine();
    }

    private static string DescribeBodies()
    {
        var sb = new StringBuilder();
        int total = 0;
        for (int i = 0; i < Bodies.Length; i++)
        {
            if (i > 0) sb.Append(" + ");
            sb.Append($"{Bodies[i].n}{Bodies[i].label}");
            total += Bodies[i].n;
        }
        sb.Append($" = {total}");
        return sb.ToString();
    }

    private static void PrintUsage()
    {
        Console.WriteLine($"FractalKernel v{VERSION} — 42-body invariant kernel");
        Console.WriteLine();
        Console.WriteLine("usage: FractalKernel <mode>");
        Console.WriteLine("  mode: fractal | dipole");
        Console.WriteLine();
        Console.WriteLine("output: fractal_<mode>_42.ppm on Desktop (or cwd)");
    }
}
