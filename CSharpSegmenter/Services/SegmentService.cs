using CSharpSegmenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSegmenter.Services
{
    public static class SegmentService
    {
        private static List<List<byte>> ExtractColourBands(List<byte[]> colours)
        {
            var numColourBand = colours[0].Length;

            var result = new List<List<byte>>(numColourBand);

            foreach (var colour in colours)
            {
                for (int i = 0; i < numColourBand; i++)
                {
                    result[i].Add(colour[i]);
                }
            }

            return result;
        }

        public static float CalculateStddev(float[] input)
        {
            float average = input.Average();
            float sumOfSquaresOfDifferences = input.Select(val => (val - average) * (val - average)).Sum();
            return (float)Math.Sqrt(sumOfSquaresOfDifferences / input.Length);

        } 

        //private static float[] Stddev(Segment segment)
        //{
        //    var colourList = segment.GetSegmentColours();
        //    var colourBandList = ExtractColourBands(colourList);
        //}
    }
}
