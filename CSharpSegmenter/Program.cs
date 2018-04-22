using CSharpSegmenter.Helpers;
using CSharpSegmenter.Models;
using CSharpSegmenter.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSegmenter
{
    class Program
    {
        static void Main(string[] args)
        {
            var image = new TiffImage("..\\TestImages\\L15-3792E-1717N-Q4.tif");

            var N = 5;
            var threshold = 800.0F;

            var segmentation = new SegmentationService(image, N, threshold);

            image.overlaySegmentation("segmentedN" + N + ".tif" , N, segmentation);


        }
    }
}
