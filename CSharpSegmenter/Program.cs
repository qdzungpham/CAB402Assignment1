using CSharpSegmenter.Helpers;
using CSharpSegmenter.Services;
using System;

namespace CSharpSegmenter
{
    class Program
    {
        static void Main(string[] args)
        {
            //throw new NotImplementedException();
            // Fixme: add implementation here

            //var image = TiffModule.LoadImage(@"C:\Development\SegmentationSkeleton\TestImages\L15-3662E-1902N-Q4.tif");
            //var v = 1;
            var c = new float[] { (float)10.0, (float)20.0, (float)30.0, (float)40.0 };
            Console.WriteLine(SegmentService.CalculateStddev(c));
            Console.ReadKey();
        }
    }
}
