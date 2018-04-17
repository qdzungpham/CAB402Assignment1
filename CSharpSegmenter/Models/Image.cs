using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSegmenter.Models
{
    public class Image
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[] Raster { get; set; }
    }
}
