using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FractalKernel;

class Program
{
    // v0.42 - full 42 body kernel, build all, dipole mode
    static void Main(string[] args)
    {
        int bodies = 42;
        string mode = args.Length > 0 ? args[0].ToLower() : "fractal";
        int width = 1920, height = 1080;
        int maxIter = 2000;

        Console.WriteLine($"FractalKernel v0.42 - mode={mode}, bodies={bodies}");

        var pixels = new byte[width * height * 3];
        var sw = Stopwatch.StartNew();

        // 42 body parallelism
        var options = new ParallelOptions { MaxDegreeOfParallelism = bodies };

        Parallel.For(0, height, options, y =>
        {
            for (int x = 0; x < width; x++)
            {
                double nx = (x - width/2.0) / (width/2.0);
                double ny = (y - height/2.0) / (height/2.0);
                int v;

                if (mode == "dipole")
                {
                    // dipole field fractal: two charges
                    v = Dipole(nx*2, ny*2, maxIter);
                }
                else
                {
                    // fractal the fractal
                    double cx = nx*1.5 - 0.7;
                    double cy = ny*1.5;
                    int i1 = Mandel(cx, cy, maxIter);
                    int i2 = Mandel(cx + (i1%100)*0.0001, cy + (i1%100)*0.0001, maxIter/2);
                    v = (i1 + i2) % 255;
                }

                int idx = (y*width + x)*3;
                pixels[idx] = (byte)v;
                pixels[idx+1] = (byte)((v*3)%255);
                pixels[idx+2] = (byte)(255-v);
            }
        });

        string name = $"fractal_{mode}_42.ppm";
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), name);
        using var fs = new FileStream(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        var header = System.Text.Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n");
        bw.Write(header);
        bw.Write(pixels);

        sw.Stop();
        Console.WriteLine($"Done {sw.Elapsed.TotalSeconds:F1}s -> {path}");
    }

    static int Mandel(double cx, double cy, int max)
    {
        double zx=0, zy=0; int i=0;
        while (zx*zx + zy*zy < 4 && i < max) { double t=zx*zx-zy*zy+cx; zy=2*zx*zy+cy; zx=t; i++; }
        return i;
    }

    static int Dipole(double x, double y, int max)
    {
        // simple dipole potential fractalization
        double r1 = Math.Sqrt((x-0.5)*(x-0.5) + y*y) + 1e-6;
        double r2 = Math.Sqrt((x+0.5)*(x+0.5) + y*y) + 1e-6;
        double potential = 1/r1 - 1/r2;
        double angle = Math.Atan2(y, x);
        // iterate to create bands
        int i=0;
        double v = potential;
        while (Math.Abs(v) < 2 && i < max) { v = v*v + Math.Sin(angle*3); i++; }
        return i % 255;
    }
}
