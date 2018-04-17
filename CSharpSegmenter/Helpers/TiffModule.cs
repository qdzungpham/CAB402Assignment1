using BitMiracle.LibTiff.Classic;
using CSharpSegmenter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSegmenter.Helpers
{
    public static class TiffModule
    {
        private static readonly int BLUE = -65536;
        // return 32 bit representation of colour of pixel (x,y) in ABGR byte order
        public static int GetColour(Image image, Coordinate coodinate)
        {
            return image.Raster[coodinate.Y * image.Width + coodinate.X];
        }

        // return list of colour components for pixel (x,y), with one entry for each colour band: red, green and blue
        public static byte[] GetColourBands(Image image, Coordinate coordinate)
        {
            int abgr = GetColour(image, coordinate);
            return new byte[] { Convert.ToByte(Tiff.GetR(abgr)),
                Convert.ToByte(Tiff.GetG(abgr)), Convert.ToByte(Tiff.GetB(abgr)) };
        }

        // create a new image by loading an existing tiff file using BitMiracle Tiff library for .NET
        public static Image LoadImage(string fileName)
        {
            var image = Tiff.Open(fileName, "r");
            var w = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            var h = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            var r = new int[w * h];
            image.ReadRGBAImage(w, h, r);
            var newImage = new Image
            {
                Width = w,
                Height = h,
                Raster = r
            };
            return newImage;
        }

        // write current image to file using BitMiracle Tiff library for .NET
        public static void SaveImage(Image image, string fileName)
        {
            var file = Tiff.Open(fileName, "w");

            // set image properties first ...
            file.SetField(TiffTag.IMAGEWIDTH, image.Width);
            file.SetField(TiffTag.IMAGELENGTH, image.Height);
            file.SetField(TiffTag.SAMPLESPERPIXEL, 4);
            file.SetField(TiffTag.COMPRESSION, Compression.LZW);
            file.SetField(TiffTag.BITSPERSAMPLE, 8);
            file.SetField(TiffTag.ROWSPERSTRIP, 1);
            file.SetField(TiffTag.ORIENTATION, Orientation.BOTLEFT);
            file.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            file.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);

            for (int y = 0; y <= image.Height - 1; y++)
            {
                var byteArray = new byte[image.Width * 4];

                for (int i = 0; i <= byteArray.Length - 1; i++)
                {
                    var x = i / 4;
                    var band = i % 4;
                    var bands = GetColourBands(image, new Coordinate { X = x, Y = y });
                    byte alpha = 255;
                    if (band == 3)
                    {
                        byteArray[i] = alpha;
                    }
                    else
                    {
                        byteArray[i] = bands[band];
                    }


                }

                file.WriteScanline(byteArray, y);
            }

            file.Close();


        }

        // draw the (top left corner of the) original image but with the segment boundaries overlayed in blue
        //public static void OverlaySegmentation(Image image, string fileName, int N, )
    }
}
