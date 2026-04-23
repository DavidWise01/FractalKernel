using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FractalKernel;

class Program
{
    static void Main()
    {
        int width = 1920, height = 1080;
        int maxIter = 2000;
        double zoom = 1.5;
        double offsetX = -0.7, offsetY = 0;

        var pixels = new byte[width * height * 3];
        var sw = Stopwatch.StartNew();

        Console.WriteLine("FractalKernel - fractal the fractal - using all cores...");
        Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, y =>
        {
            for (int x = 0; x < width; x++)
            {
                double cx = (x - width/2.0) * 4.0/width/zoom + offsetX;
                double cy = (y - height/2.0) * 4.0/width/zoom + offsetY;
                int iter1 = Mandel(cx, cy, maxIter);

                double cx2 = cx + (iter1 % 100) * 0.0001;
                double cy2 = cy + (iter1 % 100) * 0.0001;
                int iter2 = Mandel(cx2, cy2, maxIter/2);

                int v = (iter1 + iter2) % 255;
                int idx = (y*width + x)*3;
                pixels[idx] = (byte)v;
                pixels[idx+1] = (byte)((v*2)%255);
                pixels[idx+2] = (byte)((v*5)%255);
            }
        });

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "fractal_fractal.ppm");
        using var fs = new FileStream(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        var header = System.Text.Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n");
        bw.Write(header);
        bw.Write(pixels);

        sw.Stop();
        Console.WriteLine($"Done in {sw.Elapsed.TotalSeconds:F1}s -> {path}");
    }

    static int Mandel(double cx, double cy, int max)
    {
        double zx=0, zy=0; int i=0;
        while (zx*zx + zy*zy < 4 && i < max)
        {
            double tmp = zx*zx - zy*zy + cx;
            zy = 2*zx*zy + cy;
            zx = tmp;
            i++;
        }
        return i;
    }
}
